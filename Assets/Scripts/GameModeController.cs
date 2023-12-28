using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
* ----------------------------
* Script Description:
* Switches GameModes when certain speed thresholds are hit
* 
* ----------------------------
* ChangeLog:
*  15.11.2023   FM  created
*  20.11.2023   FM  added UpdateGameMode() to switch GameMode according to playerspeed hitting certain thresholds
*  21.11.2023   FM  added getter, added EventHandler to notify ProceduralGeneration whenever the GameMode changes
*  22.11.2023   FM  added proper threshold capture to switch game modes and trigger OnGameModeUpdated() as intended
*  24.11.2023   FM  added tooltips; fixed playerspeed being updated as intended; 
*                   removed event based system since it didn't work as intended
*  26.11.2023   FM  moved GameMode check
*  05.12.2023   FM  added SetCurrentGameMode()
*  12.12.2023   FM  changed threshold values in inspector
*  22.12.2023   FM  implemented auto getters / setters
*  
*  TODO: 
*      - 
*  Buglist:
*      - playerspeed is not updated as intended - resolved
*      - correct playerspeed check for game mode change - resolved
*      
*****************************************************************************/
public enum EGameModes
{
    GAMEOVER = -1,
    START = 0,  //Switching from START to VERY_EASY does NOT change prefabs in ProceduralGeneration.cs
    VERY_EASY = 1,
    EASY = 2,
    MEDIUM = 3,
    HARD = 4,
    VERY_HARD = 5,
    EXTREME = 6
}

public class GameModeController : MonoBehaviour 
{
    public static GameModeController Instance { get; private set; }

    [SerializeField]
    private GameObject playerGO;
    //Threshold describing the speed needed to switch to higher difficulty

    [SerializeField]
    public EGameModes currentGameMode { get { return m_currentGameMode; } set { m_currentGameMode = value; } }
    private EGameModes m_currentGameMode;

    private EGameModes m_nextGameMode;
    [SerializeField]
    public bool gameModeChanged { get { return m_gameModeChanged; } set { m_gameModeChanged = value; } }
    private bool m_gameModeChanged;

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

    private PlayerController m_playerControllerRef;
    private float m_playerSpeed;

    [SerializeField, Tooltip("To swap scenes when game mode is NOT one of the playmode modes")]
    private GameObject mySceneManagerGO;
    private MySceneManager m_mySceneManagerRef;


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
        if(playerGO != null) { m_playerControllerRef = playerGO.GetComponent<PlayerController>(); }
        if(mySceneManagerGO != null) { m_mySceneManagerRef = mySceneManagerGO.GetComponent<MySceneManager>(); }
    }
    private void Start()
    {
        m_currentGameMode = EGameModes.START;
        m_nextGameMode = EGameModes.VERY_EASY;
    }
    private void Update()
    {
        m_playerSpeed = m_playerControllerRef.speedVertical;
        UpdateGameMode();
        //Update nextGameMode
        if (m_nextGameMode == m_currentGameMode)
        {
            //Debug.Log("next game mode = current game mode");
            m_gameModeChanged = true;
            m_nextGameMode++;
        }
        //Debug.Log("curr game mode: " + m_currentGameMode + " next game mode: " + m_nextGameMode);
        if (m_currentGameMode == EGameModes.GAMEOVER)
        {
            m_mySceneManagerRef.LoadGameOver();
        }
    }
    
    /// <summary>
    /// Updates the game mode whenever speed thresholds are passed by the players velocity
    /// </summary>
    private void UpdateGameMode()
    {
        //rounded value needed to avoid skipping hitting a threshold
        double playerSpeedRounded = Math.Round(m_playerSpeed, 2);
        //Debug.Log(playerSpeedRounded + "   " + speedThresholdVeryEasy);
        if (playerSpeedRounded == speedThresholdVeryEasy)
        {
            m_currentGameMode = EGameModes.VERY_EASY;
            Debug.Log("Hit very easy speed threshold");
        }
        else if (playerSpeedRounded == speedThresholdEasy)
        {
            m_currentGameMode = EGameModes.EASY;
            Debug.Log("Hit easy speed threshold");
        }
        else if (playerSpeedRounded == speedThresholdMedium)
        {
            m_currentGameMode = EGameModes.MEDIUM;
            Debug.Log("Hit medium speed threshold");
        }
        else if (playerSpeedRounded == speedThresholdHard)
        {
            m_currentGameMode = EGameModes.HARD;
            Debug.Log("Hit hard speed threshold");
        }
        else if (playerSpeedRounded == speedThresholdVeryHard)
        {
            m_currentGameMode = EGameModes.VERY_HARD;
            Debug.Log("Hit very hard speed threshold");
        }
        else if(playerSpeedRounded == speedThresholdExtreme)
        {
            m_currentGameMode = EGameModes.EXTREME;
            Debug.Log("Hit extreme speed threshold");
        }
    }

}
	

