using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: DynamicDifficultyAdjuster.cs
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
 * Manipulates loaded values from GameDataManager.cs to adjust difficulty properly
 * NOTE: This works PER session and does NOT save data ACCROSS sessions so far
 * 
 * ----------------------------
 * ChangeLog:
 *  20.12.2023   FM  created
 *  
 *  TODO: 
 *      - add proper dda calculation
 *      
 *  Buglist:
 *      - 
 *  
 *****************************************************************************/
public static class DynamicDifficultyAdjuster
{
    public static EPlayerSkillLevel CurrentPlayerType { get { return s_playerType; } private set { s_playerType = value; } }
    private static EPlayerSkillLevel s_playerType = EPlayerSkillLevel.NOOB;

    private static float s_beginnerThreshold = 100f;
    private static float s_intermediateThreshold = 300f;
    private static float s_advancedThreshold = 600f;
    private static float s_expertThreshold = 1000f;

    private static float s_noobDifficultyFactor = 1f;
    private static float s_intermediateDifficultyFactor = 1.1f;
    private static float s_advancedDifficultyFactor = 1.2f;
    private static float s_expertDifficultyFactor = 1.3f;
    /// <summary>
    /// Adjusts the speedModifier by inputting the old speedModifier and updating it 
    /// according to the players participated difficulty
    /// </summary>
    /// <param name="_oldSpeedModifier">old speedModifier that is updated</param>
    /// <returns>new speedModifier</returns>
    public static float CalculateAdjustedSpeedModifier(float _oldSpeedModifier)
    {
        float default_factor = (int)s_playerType;
        float new_speed_modifier = _oldSpeedModifier + default_factor;

        return new_speed_modifier;
    }

    /// <summary>
    /// This function has to be called before starting a new game to calculate 
    /// the current playertype, so it can be used to update the difficulty accordingly
    /// </summary>
    /// <returns>calculated playertype</returns>
    /// <param name="_distanceTravelled">distance the player reached before dying</param>
    /// <param name="_maxVerticalSpeed">speed value the player reached before dying</param>
    /// <param name="_deathCounter">counts deaths throughout session</param>
    public static void UpdatePlayerType(float _distanceTravelled, int _deathCounter)
    {
        //Calculate player type with input data
        if(_deathCounter > 0)
        {
            float deathcounter_to_distance_ratio = _distanceTravelled / _deathCounter;
            Debug.Log("DC / distance: " + deathcounter_to_distance_ratio);
            if (deathcounter_to_distance_ratio < s_beginnerThreshold)
            {
                s_playerType = EPlayerSkillLevel.NOOB;
            }
            else if (deathcounter_to_distance_ratio < s_intermediateThreshold)
            {
                s_playerType = EPlayerSkillLevel.BEGINNER;
            }
            else if (deathcounter_to_distance_ratio < s_advancedThreshold)
            {
                s_playerType = EPlayerSkillLevel.INTERMEDIATE;
            }
            else if (deathcounter_to_distance_ratio < s_expertThreshold)
            {
                s_playerType = EPlayerSkillLevel.ADVANCED;
            }
            else //expert
            {
                s_playerType = EPlayerSkillLevel.EXPERT;
            }
            Debug.Log("Switched to player type: " + s_playerType);
        }
    }


}

/// <summary>
/// Categorises playertypes by skill level
/// </summary>
public enum EPlayerSkillLevel
{
    NONE = -1,
    NOOB = 0,
    BEGINNER = 1,
    INTERMEDIATE = 2,
    ADVANCED = 3,
    EXPERT = 4,
}

/// <summary>
/// Categorises PlayerType by enjoyment type
/// EASY_FUN = casual player
/// HARD_FUN = difficulty enjoying player
/// </summary>
public enum EPlayerType
{
    NONE = -1,
    EASY_FUN = 0,
    HARD_FUN = 1,
}