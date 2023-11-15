using System;
using System.Collections;
using System.Collections.Generic;
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
 *  12.11.2023   FM  adjusted Variables identifiers and names to fulfill coding conventions (see CodingConventions.txt), added manual calculation of gravity, fixed bug where Rigidbody speed was accidentally overwritten
 *  
 *  TODO: 
 *      - Scale accelleration accordingly
 *  
 *  Buglist:
 *      - Player sticking to the wall when holding left or right respectively
 *  
 *****************************************************************************/
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject ProceduralGeneratorGO;
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
    private float speedModifier;

    void Start()
    {
        m_rb = transform.GetComponent<Rigidbody>();
        m_rb.mass = c_rbMass;
        m_proceduralGeneratorRef = ProceduralGeneratorGO.GetComponent<ProceduralGeneration>();

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
        //Debug.Log(this.transform.position);
    }

    private void UpdatePlayerSpeed()
    {
        speedModifier = m_proceduralGeneratorRef.GetSpeedModifier();
        speedVertical += Time.deltaTime * c_startingAccellerationVertical * speedModifier;
        speedHorizontal += Time.deltaTime * c_startingAccellerationHorizontal * speedModifier;
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
        ApplyMovement();
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

    private void ApplyMovement()
    {
        //Setting up keyInputs, Ignoring vertical movement inputs
        Vector3 keyInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        //Apply user input horizontal movement, continous vertical movement and jump movement
        m_rb.velocity = new Vector3(speedHorizontal * keyInput.x, m_rb.velocity.y, speedVertical);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) 
        {
            isGrounded = true;
        }
        else if (collision.gameObject.CompareTag("WallLeft"))
        {

        }
        else if (collision.gameObject.CompareTag("WallRight"))
        {

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

