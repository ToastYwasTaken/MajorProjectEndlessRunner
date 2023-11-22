using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
 * 
 * ChangeLog
 * ----------------------------
 *  18.08.2023   FM  created
 *  14.09.2023   FM  removed unnecessary parts
 *  06.10.2023   FM  added horizontal continous movement, adjusted values, added seperated acceleration and speed variables
 *  12.11.2023   FM  adjusted Variables identifiers and names to fulfill coding conventions (see CodingConventions.txt), added manual calculation of gravity, fixed bug                   where Rigidbody speed was accidentally overwritten
 *  21.11.2023   FM  added null check
 *  
 *  Buglist:
 *      - Player sticking to the wall when holding left or right respectively
 *  TODO: 
 *      - Update colliders
 *****************************************************************************/
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject proceduralGeneratorGO;
    [SerializeField]
    private float speedVertical = 5f; //initial speed moving forward
    [SerializeField]
    private float speedHorizontal = 5f; //initial speed moving left and right
    [SerializeField, ReadOnly]
    private float rbVelocityY;  //velocity of rb, applied by gravity, jumping and falling / only for debugging purposes displayed
    [SerializeField, ReadOnly]
    private bool isGrounded;   
    [SerializeField, ReadOnly]
    private Vector3 scaledGravityVector; //gravity applied to the player

    private const float c_jumpForce = 10f;    //jump force multiplier
    private const float c_startingAccellerationVertical = 0.05f; //accelleration moving forward
    private const float c_rbMass = 1f;
    private const float c_fallSpeedMultiplier = 2.8f;
    private const float c_gravityConstant = -9.81f;
    private const float gravityScale = 1.5f;    //scale factor for the gravity
    private float c_startingAccellerationHorizontal;  //accelleration moving left and right
    private static Vector3 s_spawnPosition = new Vector3(0, 1.5f, 0);
    private static Quaternion s_spawnRotation = new Quaternion(0, 0, 0, 0);
    private Rigidbody m_rb;
    private ProceduralGeneration m_proceduralGeneratorRef;
    private float m_speedModifier;
    private Vector3 m_keyInput;

    private void Awake()
    {
        if (proceduralGeneratorGO == null)
        {
            proceduralGeneratorGO = GameObject.FindAnyObjectByType<ProceduralGeneration>().gameObject;
        }
    }
    void Start()
    {
        m_rb = transform.GetComponent<Rigidbody>();
        m_rb.mass = c_rbMass;
        m_proceduralGeneratorRef = proceduralGeneratorGO.GetComponent<ProceduralGeneration>();

        c_startingAccellerationHorizontal = c_startingAccellerationVertical / 2;
        //Spawn player
        transform.position = s_spawnPosition;
        transform.rotation = s_spawnRotation;
    }

    void Update()
    {
        UpdatePlayerSpeed();
        PlayerFall();
        PlayerJump();
    }

    private void UpdatePlayerSpeed()
    {
        m_speedModifier = m_proceduralGeneratorRef.GetSpeedModifier();
        speedVertical += Time.deltaTime * c_startingAccellerationVertical * m_speedModifier;
        speedHorizontal += Time.deltaTime * c_startingAccellerationHorizontal * m_speedModifier;
    }

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
    private void PlayerFall()
    {
        //calculating quicker fall
        if (m_rb.velocity.y < 0)
        {
            m_rb.velocity += Vector3.up * scaledGravityVector.y * (c_fallSpeedMultiplier - 1) * Time.deltaTime;
            rbVelocityY = m_rb.velocity.y;
        }
    }

    private void FixedUpdate()
    {
        ApplyDefaultGravity();
        //Setting up keyInputs, Ignoring vertical movement inputs
        m_keyInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        //Apply user input horizontal movement, continous vertical movement and jump movement
        m_rb.velocity = new Vector3(speedHorizontal * m_keyInput.x, rbVelocityY, speedVertical);
    }

    /// <summary>
    /// Calculate gravity by Code to add a gravityScale for quicker Falling / Jumping
    /// </summary>
    private void ApplyDefaultGravity()
    {
        scaledGravityVector = c_gravityConstant * gravityScale * Vector3.up;
        m_rb.AddForce(scaledGravityVector, ForceMode.Acceleration);
        rbVelocityY = m_rb.velocity.y;
    }

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

        }
    }

    public float GetVerticalSpeed()
    {
        return speedVertical;
    }


}

