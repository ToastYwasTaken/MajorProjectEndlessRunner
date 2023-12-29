using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: GameData.cs
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
 * The GO holding game data
 * 
 * ----------------------------
 * ChangeLog:
 *  29.12.2023   FM  created
 *  
 *  TODO: 
 *      - 
 *      
 *  Buglist:
 *      - 
 *  
 *****************************************************************************/

public class GameData : SaveableBehavior
{
    public static GameData Instance { get; private set; }
    [SerializeField]
    private GameObject playerGO;

    private const string c_distanceTravelledKey = "distance";
    private const string c_deathCounterKey = "deathCounter";

    private float m_distanceTravelled;
    private float m_deathCounter;

    /// <summary>
    /// Data that is written to disk
    /// </summary>
    public override JsonData SavedData
    {
        get
        {
            var result = new JsonData();
            m_distanceTravelled = (float)Math.Round(playerGO.GetComponent<PlayerController>().playerPosition.z, 2);
            result[c_distanceTravelledKey] = m_distanceTravelled;
            result[c_deathCounterKey] = ++m_deathCounter;  //incrementing death counter
            Debug.Log("Saving distanceTravelled: " + m_distanceTravelled + " and deathcounter: " + m_deathCounter);
            return result;
        }
    }
    /// <summary>
    /// Loads data from previous game
    /// additional cast needed bc sh JsonData can't directly convert to float
    /// </summary>
    /// <param name="data">data to load here</param>
    public override void LoadFromData(JsonData data)
    {
        if (data.ContainsKey(c_distanceTravelledKey))
        {
            m_distanceTravelled = (float)((double)data[c_distanceTravelledKey]);
        }
        if (data.ContainsKey(c_deathCounterKey))
        {
            m_deathCounter = (int)(data[c_deathCounterKey]);
        }
        Debug.Log("Loaded values BEFORE DDA - distanceTravelled: " + m_distanceTravelled + " deathcounter: " + m_deathCounter);
        //m_speedModifier = DynamicDifficultyAdjuster.CalculateAdjustedSpeedModifier(m_speedModifier);
        //DynamicDifficultyAdjuster.UpdatePlayerType(m_distanceTravelledLoaded,  m_deathCounter);
        //Debug.Log("Adjusted speed modifier AFTER DDA: " + m_speedModifier);
    }

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
    }
}
