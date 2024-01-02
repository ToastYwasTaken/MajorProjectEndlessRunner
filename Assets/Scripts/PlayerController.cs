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
 *  22.12.2023   FM  Fixed getters / setters causing issues
 *  26.12.2023   FM  Removed speedVertical from Data saving
 *  29.12.2023   FM  Removed implementation of SaveableBehaviour; moved data saving to SavingService.cs
 *  
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
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject gameModeControllerGO;
    [SerializeField, Tooltip("Speed moving left and right, depending on player input")]
    private float speedHorizontal = 9f; 
    [SerializeField, Tooltip("Jump / falling velocity applied to the rb")]
    private float rbVelocityY; 
    [SerializeField, Tooltip("Bool for jumping behavior")]
    private bool isGrounded;   
    [SerializeField, Tooltip("Scales gravity to accellerate player fall speed")]
    private Vector3 scaledGravityVector;
    [Tooltip("Speed moving forward")]
    public float SpeedVertical { get { return m_speedVertical; } set { m_speedVertical = value; } }
    private float m_speedVertical = 9f;


    #region constants
    private const float c_jumpForce = 10f;    //jump force multiplier
    private const float c_startingAccellerationVertical = 0.05f; //accelleration moving forward
    private const float c_rbMass = 1f;
    private const float c_fallspeedMultiplier = 2.8f;
    private const float c_gravityConstant = -9.81f;
    private const float c_gravityScale = 1.5f;    //scale factor for the gravity
    #endregion

    #region DDA 
    [Tooltip("Player position")]
    public Vector3 PlayerPosition { get { return m_playerPosition; } set { m_playerPosition = value; } }
    private Vector3 m_playerPosition;
    private float m_speedModifier;
    [Tooltip("Death counter")]
    public int DeathCounter { get { return m_deathCounter; } set { m_deathCounter = value; } }

    private int m_deathCounter = 0;
    [Tooltip("Player type")]
    public EPlayerType PlayerType { get { return m_playerType; } set { m_playerType = value; } }

    private EPlayerType m_playerType = EPlayerType.NONE;
    [Tooltip("Player skill level")]
    public EPlayerSkillLevel PlayerSkillLevel { get { return m_playerSkillLevel; } set { m_playerSkillLevel = value; } }

    private EPlayerSkillLevel m_playerSkillLevel = EPlayerSkillLevel.NONE;
    #endregion

    #region other
    private float m_startingAccellerationHorizontal;  //accelleration moving left and right
    private Rigidbody m_rb;
    private GameModeController m_gameModeControllerRef;
    private Vector3 m_keyInput;
    public static PlayerController Instance { get; private set; }
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
        //Load DDA values
        m_deathCounter = DynamicDifficultyAdjuster.DeathCounter;
        m_speedModifier = DynamicDifficultyAdjuster.SpeedModifier;
        m_playerSkillLevel = DynamicDifficultyAdjuster.PlayerSkillLevel;
        m_playerType = DynamicDifficultyAdjuster.PlayerType;
        Debug.Log("DC: " + m_deathCounter + " - speed mod: " + m_speedModifier + " - playerSkill: " + m_playerSkillLevel + " - playerType: " + m_playerType);
    }

    void Update()
    {
        m_playerPosition = transform.position;
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
        m_rb.velocity = new Vector3(speedHorizontal * m_keyInput.x, rbVelocityY, m_speedVertical);
    }

    private void UpdatePlayerSpeed()
    {
        m_speedVertical += Time.deltaTime * c_startingAccellerationVertical * m_speedModifier;
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
            m_gameModeControllerRef.currentGameMode = EGameModes.GAMEOVER;
        }
    }

}

