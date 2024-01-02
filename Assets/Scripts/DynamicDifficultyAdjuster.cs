using System;
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
 *  31.12.2023   FM  edited methods; implemented UpdateDifficulty() properly;
 *                   added public accessible fields for the adjusted values
 *  02.01.2023   FM  implemented CalculatePlayerType()
 *  
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
    public static EPlayerType PlayerType { get { return s_playerType; } private set { s_playerType = value; } }
    private static EPlayerType s_playerType = EPlayerType.NONE;

    public static EPlayerSkillLevel PlayerSkillLevel { get { return s_playerSkillLevel; } private set { s_playerSkillLevel = value; } }
    private static EPlayerSkillLevel s_playerSkillLevel = EPlayerSkillLevel.NONE;

    public static float SpeedModifier { get { return s_speedModifier; } private set { s_speedModifier = value; } }
    private static float s_speedModifier = 1.0f;

    public static int DeathCounter { get { return s_deathCounter; } private set { s_deathCounter = value; } }
    private static int s_deathCounter = 0;
    public static float ObstacleDensity { get { return s_obstacleDensity; } private set { s_obstacleDensity = value; } }
    private static float s_obstacleDensity = 1f;
    public static int TimesLaunched { get { return s_timesLaunched; } private set { s_timesLaunched = value; } }
    private static int s_timesLaunched = 0;

    private static float s_distanceReachedAverage;

    private const float c_beginnerThreshold = 600f; //about the distance to get to EASY (blue templates)
    private const float c_intermediateThreshold = 1200f;
    private const float c_advancedThreshold = 2500f;
    private const float c_expertThreshold = 4000f;
    private const int c_minAmountOfSaveFilesToUpdatePlayerSkillLevel = 1;
    private const int c_minAmountOfSaveFilesToUpdatePlayerType = 4;

    private const int c_weight5 = 5;
    private const int c_weight10 = 10;
    private const int c_weight30 = 30;



    /// <summary>
    /// Updates the difficulty according to the saved game data
    /// </summary>
    public static void UpdateDifficulty()
    {
        //Load saved data
        (float[], int[], EPlayerType[], EPlayerSkillLevel[], int[]) game_data = SavingService.LoadData();

        //Do skill and type adjustments only if minAmount of save files exist
        if (game_data.Item1.Length > c_minAmountOfSaveFilesToUpdatePlayerSkillLevel)
        {
            //Calculate average distance reached
            s_distanceReachedAverage = CalculateDistanceAverage(game_data.Item1);
            //Get values assigned to player types and skill levels
            PlayerSkillLevel = CalculatePlayerSkillLevel(s_distanceReachedAverage, game_data.Item4);
            Debug.Log("Updated player skill level: " + PlayerSkillLevel);
            if (game_data.Item1.Length > c_minAmountOfSaveFilesToUpdatePlayerType)
            {
                PlayerType = CalculatePlayerType(s_distanceReachedAverage, game_data.Item2, game_data.Item3, game_data.Item4, game_data.Item5);
                Debug.Log("Updated player type: " + PlayerType);
            }
            (float, float) player_type_and_skill_level_factor = CalculateFactors(PlayerType, PlayerSkillLevel);
            //Update values accessed by game objects
            UpdateSpeedModifier(player_type_and_skill_level_factor.Item1, player_type_and_skill_level_factor.Item2);
            UpdateObstacleDensity(player_type_and_skill_level_factor.Item1, player_type_and_skill_level_factor.Item2);
        }
        //Update death counter
        if (game_data.Item2.Length > 0)
        {
            DeathCounter = game_data.Item2[game_data.Item2.Length - 1];
        }
    }

    /// <summary>
    /// Assigns values to player type and skill level to increase / decrease difficulty accordingly
    /// </summary>
    /// <param name="_playerType">player type</param>
    /// <param name="_playerSkillLevel">player skill level</param>
    /// <returns>float factors representing player type and skill level</returns>
    private static (float, float) CalculateFactors(EPlayerType _playerType, EPlayerSkillLevel _playerSkillLevel)
    {
        float player_type_factor = 1.0f;
        switch (_playerType)
        {
            case EPlayerType.NONE:
                player_type_factor = 1.0f;
                break;
            case EPlayerType.EASY_FUN:
                player_type_factor = 0.8f;
                break;
            case EPlayerType.HARD_FUN:
                player_type_factor = 1.2f;
                break;
            default:
                break;
        }
        float player_skill_factor = 1.0f;
        switch (_playerSkillLevel)
        {
            case EPlayerSkillLevel.NONE: player_skill_factor = 1.0f;
                break;
            case EPlayerSkillLevel.NOOB: player_skill_factor = 1.1f;
                break;
            case EPlayerSkillLevel.BEGINNER: player_skill_factor = 1.2f;
                break;
            case EPlayerSkillLevel.INTERMEDIATE: player_skill_factor = 1.3f;
                break;
            case EPlayerSkillLevel.ADVANCED: player_skill_factor = 1.4f;
                break;
            case EPlayerSkillLevel.EXPERT: player_skill_factor = 1.5f;
                break;
            default:
                break;
        }
        return (player_type_factor, player_skill_factor);
    }


    /// <summary>
    /// Adjusts the speedModifier by inputting the old speedModifier and updating it 
    /// according to the player type and skill level
    /// Only updates the modifier if a non default playerType and playerSkillLevel are given
    /// </summary>
    private static void UpdateSpeedModifier(float _playerTypeFactor, float _playerSkillFactor)
    {
        SpeedModifier = _playerTypeFactor * _playerSkillFactor;
        Debug.Log("New speed mod: " + SpeedModifier);
    }

    /// <summary>
    /// Adjusts the obstacleDensity by inputting the old speedModifier and updating it 
    /// according to the player type and skill level
    /// Only updates the modifier if a non default playerType and playerSkillLevel are given
    /// </summary>
    private static void UpdateObstacleDensity(float _playerTypeFactor, float _playerSkillFactor)
    {
        ObstacleDensity = _playerTypeFactor * _playerSkillFactor;
        Debug.Log("New speed mod: " + SpeedModifier);
    }

    /// <summary>
    /// Calculates distance average
    /// </summary>
    /// <param name="_distancesReachedArr">average of all distances travelled</param>
    /// <returns>average distance</returns>
    private static float CalculateDistanceAverage(float[] _distancesReachedArr)
    {
        //Get average distance reached
        int counter = 0;
        for (int i = 0; i < _distancesReachedArr.Length; i++)
        {
            s_distanceReachedAverage += _distancesReachedArr[i];
            counter++;
        }
        if (counter != 0)
        {
            s_distanceReachedAverage /= counter;
        }
        Debug.Log("Distance average: " + s_distanceReachedAverage);
        return s_distanceReachedAverage;
    }

    /// <summary>
    /// Calculate player type with input data by average reached distance
    /// </summary>
    private static EPlayerSkillLevel CalculatePlayerSkillLevel(float _distanceReachedAverage, EPlayerSkillLevel[] _playerSkillLevelArr)
    {
        if (s_distanceReachedAverage < c_beginnerThreshold)
        {
            return EPlayerSkillLevel.NOOB;
        }
        else if (s_distanceReachedAverage < c_intermediateThreshold)
        {
            return EPlayerSkillLevel.BEGINNER;
        }
        else if (s_distanceReachedAverage < c_advancedThreshold)
        {
            return EPlayerSkillLevel.INTERMEDIATE;
        }
        else if (s_distanceReachedAverage < c_expertThreshold)
        {
            return EPlayerSkillLevel.ADVANCED;
        }
        else return EPlayerSkillLevel.EXPERT;
    }

    /// <summary>
    /// This function has to be called before starting a new game to calculate 
    /// the current playertype, so it can be used to update the difficulty accordingly
    /// </summary>
    private static EPlayerType CalculatePlayerType(float _distanceReachedAverage, int[] _deathCounterArr, EPlayerType[] _playerTypeArr, EPlayerSkillLevel[] _playerSkillArr, int[] _timesLaunched)
    {
        EPlayerType new_player_type = EPlayerType.NONE;
        EPlayerType last_player_type = _playerTypeArr[_playerTypeArr.Length-1];
        EPlayerSkillLevel last_player_skill_level = _playerSkillArr[_playerSkillArr.Length-1];
        //get an earlier value in the skill array if the array is long enough
        int earlier_player_skill_level_index = _playerSkillArr.Length - 3 >= 0 ? _playerSkillArr.Length - 3 : _playerSkillArr.Length;
        EPlayerSkillLevel earlier_player_skill_level = _playerSkillArr[earlier_player_skill_level_index];
        //TODO: ADD times launched / death counter factor
        int last_death_counter = _deathCounterArr[_deathCounterArr.Length - 1];
        int probabilty = 50;
        //Assign higher probabilities to keep the previously calculated player type
        if (last_player_type == EPlayerType.EASY_FUN)
        {
            probabilty+=c_weight30;
        }else if(last_player_type == EPlayerType.HARD_FUN)
        {
            probabilty-=c_weight30;
        }
        //Increases probability for HARD_FUN if last skill level was ADVANCED or higher
        //Decreases probability for EASY_FUN if last skill level was BEGINNER or lower
        probabilty += ((int)last_player_skill_level - 2) * c_weight10;
        //Same as previous step, with an earlier calculated skill level and lower weights
        probabilty += ((int) earlier_player_skill_level - 2) * c_weight5;
        //Increase probability for HARD_FUN if distanceReachedAverage is higher than intermediateThreshold
        probabilty += (_distanceReachedAverage >= c_intermediateThreshold ? 1 : -1) * c_weight10;
        //Calculate playerType
        new_player_type = (probabilty < 50) ? (PlayerType = EPlayerType.EASY_FUN) : (PlayerType = EPlayerType.HARD_FUN);
        return new_player_type;
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
    EASY_FUN = 1,
    HARD_FUN = 2,
}

