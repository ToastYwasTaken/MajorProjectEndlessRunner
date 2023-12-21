using System;
using LitJson;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: PlayerController.cs
 * Version: 1.0
 * Autor: Franz M?rike (FM)
 * 
 * These coded instructions, statements, and computer programs contain
 * proprietary information of the author and are protected by Federal
 * copyright law. They may not be disclosed to third parties or copied
 * or duplicated in any form, in whole or in part, without the prior
 * written consent of the author.
 * ----------------------------
 * Script Description:
 * Controls player movement and behaviour
 * 
 * ----------------------------
 * ChangeLog:
 *  18.08.2023   FM  created
 *  14.09.2023   FM  removed unnecessary parts
 *  06.10.2023   FM  added horizontal continous movement, adjusted values, added seperated acceleration and speed variables
 *  12.11.2023   FM  adjusted Variables identifiers and names to fulfill coding conventions (see CodingConventions.txt), added manual calculation of gravity, fixed bug                   
 *                   where Rigidbody speed was accidentally overwritten
 *  21.11.2023   FM  added null check
 *  22.11.2023   FM  added getter
 *  24.11.2023   FM  removed reference to ProceduralGeneration as of now; added tooltips
 *  27.11.2023   FM  minor changes
 *  05.12.2023   FM  added gameover checks on colliders
 *  12.12.2023   FM  changed speed values in inspector
 *  18.12.2023   FM  added saving and loading for speedmodifier
 *  19.12.2023   FM  minor changes
 *  21.12.2023   FM  Updated getters / setters; fixed implementation of SavableBehaviour by correcting 
 *                   deserialization of JsonData objects; removed SerializeValue() and DeserializeValue()
 *                  
 *  TODO: 
 *      - Implement colliders switching GameModes - done
 *      - Fix speed increase to work properly - done
 *      - Correct type conversion of JsonData to my required data types - done
 *      
 *  Buglist:
 *      - Player sticking to the wall when holding left or right respectively
 *  
 *****************************************************************************/
public class PlayerController : SaveableBehavior
{
 
    [SerializeField]
    private GameObject gameModeControllerGO;
    [SerializeField, Tooltip("Speed moving left and right, depending on player input")]
    private float speedHorizontal = 5f; 
    [SerializeField, Tooltip("Jump / falling velocity applied to the rb")]
    private float rbVelocityY; 
    [SerializeField, Tooltip("Bool for jumping behavior")]
    private bool isGrounded;   
    [SerializeField, Tooltip("Scales gravity to accellerate player fall speed")]
    private Vector3 scaledGravityVector;

    [Tooltip("player position")]
    public Vector3 playerPosition { get { return m_playerPosition; } set { m_playerPosition = value; } }
    [Tooltip("Speed moving forward")]
    public float speedVertical { get { return m_verticalSpeed; } set { m_verticalSpeed = value; } }

    #region constants
    private const float c_jumpForce = 10f;    //jump force multiplier
    private const float c_startingAccellerationVertical = 0.05f; //accelleration moving forward
    private const float c_rbMass = 1f;
    private const float c_fallspeedMultiplier = 2.8f;
    private const float c_gravityConstant = -9.81f;
    private const float c_gravityScale = 1.5f;    //scale factor for the gravity
    #endregion

    #region DDA 
    private float m_distanceTravelled;
    private float m_speedModifier = 1.0f;
    private int m_deathCounter = 0;
    #endregion

    #region other
    private Vector3 m_playerPosition;
    private float m_verticalSpeed;
    private float m_startingAccellerationHorizontal;  //accelleration moving left and right
    private Rigidbody m_rb;
    private GameModeController m_gameModeControllerRef;
    private Vector3 m_keyInput;
    public static PlayerController Instance { get; private set; }
    #endregion

    #region Data Saving
    private const string c_speedmodifierKey = "speedmodifier";
    private const string c_distanceTravelledKey = "distance";
    private const string c_deathCounterKey = "deathCounter";

    /// <summary>
    /// Data that is written to disk
    /// </summary>
    public override JsonData SavedData
    {
        get
        {
            var result = new JsonData();
            result[c_speedmodifierKey] = m_speedModifier; 
            result[c_distanceTravelledKey] = m_distanceTravelled;
            result[c_deathCounterKey] =  m_deathCounter++;  //incrementing death counter
            Debug.Log("Saving speedMod: " + m_speedModifier + " distanceTravelled: " + m_distanceTravelled + " and deathcounter: " + m_deathCounter);
            return result;
        }
    }
    /// <summary>
    /// Loads data from previous game
    /// </summary>
    /// <param name="data">data to load here</param>
    public override void LoadFromData(JsonData data)
    {
        if (data.ContainsKey(c_speedmodifierKey))
        {
            m_speedModifier = (float)((double)data[c_speedmodifierKey]);
        }
        if (data.ContainsKey(c_distanceTravelledKey))
        {
            m_distanceTravelled = (float)((double)data[c_distanceTravelledKey]);
        }
        if (data.ContainsKey(c_deathCounterKey))
        {
            m_deathCounter = (int)(data[c_deathCounterKey]);
        }
        Debug.Log("Loaded values BEFORE DDA - speedMod: " + m_speedModifier + " distanceTravelled: " + m_distanceTravelled + " deathcounter: " + m_deathCounter);
        m_speedModifier = DynamicDifficultyAdjuster.CalculateAdjustedSpeedModifier(m_speedModifier, m_distanceTravelled, m_deathCounter);
        Debug.Log("Loaded values AFTER DDA - speedMod: " + m_speedModifier + " distanceTravelled: " + m_distanceTravelled + " deathcounter: " + m_deathCounter);
    }
    #endregion

    private void Awake()
    {
        //Singleton checks
        if (Instance == null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
        if (gameModeControllerGO != null) { m_gameModeControllerRef = gameModeControllerGO.GetComponent<GameModeController>(); }
    }
    void Start()
    {
        m_rb = transform.GetComponent<Rigidbody>();
        m_rb.mass = c_rbMass;

        m_startingAccellerationHorizontal = c_startingAccellerationVertical / 2;
        //Set player spawn values
        transform.position = new Vector3(0,1.5f,0);
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        m_distanceTravelled = transform.position.z;
        UpdatePlayerSpeed();
        PlayerFall();
        PlayerJump();
    }
    private void FixedUpdate()
    {
        ApplyDefaultGravity();
        //Setting up keyInputs, Ignoring vertical movement inputs
        m_keyInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        //Apply user input horizontal movement, continous vertical movement and jump movement
        m_rb.velocity = new Vector3(speedHorizontal * m_keyInput.x, rbVelocityY, speedVertical);
    }

    private void UpdatePlayerSpeed()
    {
        speedVertical += Time.deltaTime * c_startingAccellerationVertical * m_speedModifier;
        speedHorizontal += Time.deltaTime * m_startingAccellerationHorizontal * m_speedModifier;
    }

    /// <summary>
    /// Calculate jump force on rb
    /// </summary>
    private void PlayerJump()
    {
        //Allows jump if pressing 'space' and grounded
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            m_rb.velocity = Vector3.up * c_jumpForce;
            isGrounded = false;
            rbVelocityY = m_rb.velocity.y;
        }
    }

    /// <summary>
    /// Calculate constant fall force on rb
    /// </summary>
    private void PlayerFall()
    {
        //calculating quicker fall
        if (m_rb.velocity.y < 0)
        {
            m_rb.velocity += Vector3.up * scaledGravityVector.y * (c_fallspeedMultiplier - 1) * Time.deltaTime;
            rbVelocityY = m_rb.velocity.y;
        }
    }

    /// <summary>
    /// Calculate gravity by Code to add a gravityScale for quicker Falling / Jumping
    /// </summary>
    private void ApplyDefaultGravity()
    {
        scaledGravityVector = c_gravityConstant * c_gravityScale * Vector3.up;
        m_rb.AddForce(scaledGravityVector, ForceMode.Acceleration);
        rbVelocityY = m_rb.velocity.y;
    }

    /// <summary>
    /// Triggers conditions when player hits other colliders
    /// </summary>
    /// <param name="collision">collision param of gameobject colliding with player's collider</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) 
        {
            isGrounded = true;
        }else if(collision.gameObject.CompareTag("WallLeft"))
        {
            rbVelocityY = 0f;
        }
        else if (collision.gameObject.CompareTag("WallRight"))
        {
            rbVelocityY = 0f;
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            m_deathCounter++;
            m_gameModeControllerRef.SetCurrentGameMode(EGameModes.GAMEOVER);
        }
    }


}

