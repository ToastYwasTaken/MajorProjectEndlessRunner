using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: PlayerController.cs
 * Version: 1.12
 * Autor: Franz M?rike (FM)
 * Note: This script is based on a group project GPA4300 with Ren? Kraus (RK), Franz M?rike (FM) and Jan Pagel (JP). 
 * The script was modified as stated in the ChangeLog to suit this project.
 * 
 * These coded instructions, statements, and computer programs contain
 * proprietary information of the author and are protected by Federal
 * copyright law. They may not be disclosed to third parties or copied
 * or duplicated in any form, in whole or in part, without the prior
 * written consent of the author.
 * 
 * ChangeLog
 * ----------------------------
 *  18.08.2022   FM  created
 *  14.09.2023   FM  removed unnecessary parts
 *  06.10.2023   FM  added horizontal continous movement, adjusted values, added seperated acceleration and speed variables
 *  
 *****************************************************************************/
public class PlayerController : MonoBehaviour
{
    //Variables
    [SerializeField]
    float RBMass = 50f;
    [SerializeField]
    float SpeedVertical = 5f;
    [SerializeField]
    float SpeedHorizontal = 5f;
    //TODO: FineTune these values:
    [SerializeField]
    float JumpForce = 200f;
    [SerializeField]
    float AccellerationVertical = 0.1f;
    [SerializeField]
    float AccellerationHorizontal = 0.05f;

    [ReadOnly]
    public bool m_pIsGrounded;

    private Vector3 m_pSpawnPosition = new Vector3(0, 1.5f, 0);
    private Vector3 m_moveDirection;
    private Quaternion m_pSpawnRotation = new Quaternion(0, 0, 0, 0);
    private Rigidbody m_RB;
    private Transform m_orientation;
    private float m_fallspeed;


    void Start()
    {
        m_RB = transform.GetComponent<Rigidbody>();
        m_RB.mass = RBMass;
        m_orientation = transform.GetChild(1).GetComponent<Transform>();
        transform.position = m_pSpawnPosition;
        transform.rotation = m_pSpawnRotation;
    }

    void Update()
    {
        Jump();
        Fall();
    }

    private void Jump()
    {
        //Updating speed values
        SpeedVertical += Time.deltaTime * AccellerationVertical;
        SpeedHorizontal += Time.deltaTime * AccellerationHorizontal;

        if (Input.GetKeyDown(KeyCode.Space) && m_pIsGrounded)
        {
            m_RB.AddForce(transform.up * JumpForce, ForceMode.Impulse);
            m_pIsGrounded = false;
        }
    }
    private void Fall()
    {
        m_fallspeed = m_RB.velocity.y + Physics.gravity.y * Time.deltaTime;

    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        //Setting up keyInputs, Ignoring vertical movement inputs
        Vector3 keyInput = new Vector3(Input.GetAxis("Horizontal"), 0, 0);

        m_moveDirection =  m_orientation.right * keyInput.x;
        //Apply user input horizontal movement and apply continous vertical movement
        m_RB.velocity = m_moveDirection * SpeedHorizontal + m_orientation.forward * SpeedVertical;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) m_pIsGrounded = true;
    }
}

