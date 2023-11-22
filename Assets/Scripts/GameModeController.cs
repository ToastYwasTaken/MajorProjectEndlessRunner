using System;
using System.Collections;
using System.Collections.Generic;
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
 *  
 *  Buglist:
 *      - playerspeed is not updated as intended
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
    [SerializeField]
    private float speedThresholdVeryEasy;
    [SerializeField]
    private float speedThresholdEasy;
    [SerializeField]
    private float speedThresholdMedium;
    [SerializeField]
    private float speedThresholdHard;
    [SerializeField]
    private float speedThresholdVeryHard;
    [SerializeField]
    private float speedThresholdExtreme;

    private GameModes m_currentGameMode;
    private GameModes m_nextGameMode;
    private PlayerController m_playerController;
    private float m_playerSpeed;

    public delegate void GameModeAction();
    public static event GameModeAction OnGameModeUpdated;

    private void Awake()
    {
        if (m_playerController == null)
        {
        m_playerController = playerGO.GetComponent<PlayerController>();
        }
    }
    private void Start()
    {
        m_currentGameMode = GameModes.START;
        m_nextGameMode = GameModes.VERY_EASY;
    }
    private void Update()
    {
        m_playerSpeed = m_playerController.GetVerticalSpeed();
        UpdateGameMode();
        if (m_nextGameMode == m_currentGameMode)
        {
        OnGameModeUpdated();
        }
    }

    private void UpdateGameMode()
    {
        //rounded value needed to avoid skipping hitting a threshold
        double playerSpeedRounded = Math.Round(m_playerSpeed, 2);
        Debug.Log(m_playerSpeed + " " + playerSpeedRounded + "   " + speedThresholdVeryEasy);
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
	

