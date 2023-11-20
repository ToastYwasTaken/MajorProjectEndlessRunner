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
 *  
 *****************************************************************************/
public enum GameModes
{
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
    private PlayerController m_playerController;
    private float m_playerSpeed;

    private void Start()
    {
        m_currentGameMode = GameModes.START;
        m_playerController = playerGO.GetComponent<PlayerController>();
    }
    private void Update()
    {
        UpdateGameMode();
    }

    private void UpdateGameMode()
    {
        m_playerSpeed = m_playerController.GetVerticalSpeed();
        if(m_playerSpeed < speedThresholdEasy)
        {
            m_currentGameMode = GameModes.VERY_EASY;
        }else if (m_playerSpeed < speedThresholdMedium)
        {
            m_currentGameMode = GameModes.EASY;
        }else if (m_playerSpeed < speedThresholdHard)
        {
            m_currentGameMode= GameModes.MEDIUM;
        }
        else if (m_playerSpeed < speedThresholdVeryHard)
        {
            m_currentGameMode = GameModes.HARD;
        }
        else if (m_playerSpeed < speedThresholdExtreme)
        {
            m_currentGameMode = GameModes.VERY_HARD;
        }
        else 
        {
            m_currentGameMode = GameModes.EXTREME;
        }
    }


}
	

