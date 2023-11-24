using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.RestService;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: GameModeController.cs
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
 *  15.11.2023   FM  created
 *  20.11.2023   FM  added UpdateGameMode() to switch GameMode according to playerspeed hitting certain thresholds
 *  21.11.2023   FM  added getter, added EventHandler to notify ProceduralGeneration whenever the GameMode changes
 *  22.11.2023   FM  added proper threshold capture to switch game modes and trigger OnGameModeUpdated() as intended
 *  24.11.2023   FM  added tooltips; fixed playerspeed being update as intended; 
 *  
 *  TODO: 
 *      - correct playerspeed check for game mode change
 *  
 *  Buglist:
 *      - playerspeed is not updated as intended - resolved
 *      
 *****************************************************************************/
public enum GameModes
{
    GAMEOVER = -1,
    START = 0,
    VERY_EASY = 1,
    EASY = 2,
    MEDIUM = 3,
    HARD = 4,
    VERY_HARD = 5,
    EXTREME = 6
}

public class GameModeController : MonoBehaviour 
{
    [SerializeField]
    private GameObject playerGO;
    //Threshold describing the speed needed to switch to higher difficulty
    [SerializeField, Tooltip("Threshold to be hit to enter VeryEasy Mode (This should be active from the start)")]
    private float speedThresholdVeryEasy;
    [SerializeField, Tooltip("Threshold to be hit to enter Easy Mode")]
    private float speedThresholdEasy;
    [SerializeField, Tooltip("Threshold to be hit to enter Medium Mode")]
    private float speedThresholdMedium;
    [SerializeField, Tooltip("Threshold to be hit to enter Hard Mode")]
    private float speedThresholdHard;
    [SerializeField, Tooltip("Threshold to be hit to enter Very Hard Mode")]
    private float speedThresholdVeryHard;
    [SerializeField, Tooltip("Threshold to be hit to enter Extreme Mode")]
    private float speedThresholdExtreme;

    private GameModes m_currentGameMode;
    private GameModes m_nextGameMode;
    private PlayerController m_playerControllerRef;
    private float m_playerSpeed;

    public delegate void GameModeAction();
    public static event GameModeAction OnGameModeUpdated;

    public static GameModeController m_instance { get; private set; }

    private void Awake()
    {
        //Singleton checks
        if (m_instance == null && m_instance != this)
        {
            Destroy(m_instance);
        }
        else
        {
            m_instance = this;
        }
        m_playerControllerRef = playerGO.GetComponent<PlayerController>();
    }
    private void Start()
    {
        m_currentGameMode = GameModes.START;
        m_nextGameMode = GameModes.VERY_EASY;
    }
    private void Update()
    {
        m_playerSpeed = m_playerControllerRef.GetVerticalSpeed();
        UpdateGameMode();
        if (m_nextGameMode == m_currentGameMode)
        {
            Debug.Log("Updating GameMode");
            //trigger event to update future spawned templates
            OnGameModeUpdated();
            //increment gameMode
            m_nextGameMode = m_currentGameMode++;
        }
    }

    private void UpdateGameMode()
    {
        //rounded value needed to avoid skipping hitting a threshold
        double playerSpeedRounded = Math.Round(m_playerSpeed, 2);
        //Debug.Log(m_playerSpeed + " " + playerSpeedRounded + "   " + speedThresholdVeryEasy);
        if (playerSpeedRounded == speedThresholdVeryEasy)
        {
            m_currentGameMode = GameModes.VERY_EASY;
            Debug.Log("Hit very easy speed threshold");
        }
        else if (playerSpeedRounded == speedThresholdEasy)
        {
            m_currentGameMode = GameModes.EASY;
        }
        else if (playerSpeedRounded == speedThresholdMedium)
        {
            m_currentGameMode = GameModes.MEDIUM;
        }
        else if (playerSpeedRounded == speedThresholdHard)
        {
            m_currentGameMode = GameModes.HARD;
        }
        else if (playerSpeedRounded == speedThresholdVeryHard)
        {
            m_currentGameMode = GameModes.VERY_HARD;
        }
        else if(playerSpeedRounded == speedThresholdExtreme)
        {
            m_currentGameMode = GameModes.EXTREME;
        }
    }

    public GameModes GetCurrentGameMode()
    {
        return m_currentGameMode;
    }

}
	

