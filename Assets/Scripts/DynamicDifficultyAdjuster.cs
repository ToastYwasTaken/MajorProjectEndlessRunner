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
 *  09.01.2023   FM  fixed minor issues, updated CalculatePlayerType()
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
    private static float s_obstacleDensity = 1.0f;
    public static int TimesLaunched { get { return s_timesLaunched; } private set { s_timesLaunched = value; } }
    private static int s_timesLaunched = 0;

    private static float s_distanceReachedAverage;

    private const float c_beginnerThreshold = 600f;     // = about the distance to get to EASY (blue templates)
    private const float c_intermediateThreshold = 1200f;
    private const float c_advancedThreshold = 2500f;
    private const float c_expertThreshold = 4000f;
    private const int c_minAmountOfSaveFilesToUpdatePlayerSkillLevel = 1;   //after min x save files the player skill level is generated
    private const int c_minAmountOfSaveFilesToUpdatePlayerType = 4;     //after min x save files the player type is generated
    private const float c_obstacleDensityFactor = 1.05f;    //determines the scale of relative increase of the obstacleDenisty
    private const float c_speedModifierFactor = 1.1f;      //determines the scale of relative increase of the obstacleDenisty




    /// <summary>
    /// Updates the difficulty according to the saved game data
    /// </summary>
    public static void UpdateDifficulty()
    {
        //Step 1: Load saved data
        (float[], int[], EPlayerType[], EPlayerSkillLevel[], int[]) game_data = SavingService.LoadData();

        //Step 2 and 3: Calculate updated values and assign them locally
        //Do skill and type adjustments only if minAmount of save files exist
        if (game_data.Item1.Length > c_minAmountOfSaveFilesToUpdatePlayerSkillLevel)
        {
            //Calculate average distance reached
            s_distanceReachedAverage = CalculateDistanceAverage(game_data.Item1);
            //Assign PlayerSkillLevel
            PlayerSkillLevel = CalculatePlayerSkillLevel(s_distanceReachedAverage, game_data.Item4);
            Debug.Log("Updated player skill level: " + PlayerSkillLevel);
            if (game_data.Item1.Length > c_minAmountOfSaveFilesToUpdatePlayerType)
            {
                //Assign PlayerType
                PlayerType = CalculatePlayerType(s_distanceReachedAverage, game_data.Item2, game_data.Item3, game_data.Item4, game_data.Item5);
                Debug.Log("Updated player type: " + PlayerType);
            }
            //Step 4: Update DDA factors accessed by other classes
            if (PlayerType != EPlayerType.NONE && PlayerSkillLevel != EPlayerSkillLevel.NONE)
            {
                (float, float) player_type_and_skill_level_factor = CalculateFactors(PlayerType, PlayerSkillLevel);
                //Step 5: Assign values accessed by game objects
                SpeedModifier = CalculateModifier(player_type_and_skill_level_factor.Item1, player_type_and_skill_level_factor.Item2, c_speedModifierFactor);
                ObstacleDensity = CalculateModifier(player_type_and_skill_level_factor.Item1, player_type_and_skill_level_factor.Item2, c_obstacleDensityFactor);
            }
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
    /// Adjusts the input modifier byupdating it according to the player type and skill level
    /// Only updates the modifier if a non default playerType and playerSkillLevel are given
    /// </summary>
    private static float CalculateModifier(float _playerTypeFactor, float _playerSkillFactor, float _modifierFactor)
    {
        float modifier = (_playerTypeFactor * _playerSkillFactor) >1 
            ? modifier = (_playerTypeFactor * _playerSkillFactor * _modifierFactor) 
            : (_playerTypeFactor * _playerSkillFactor / _modifierFactor);
        //Debug.Log("New speed mod: " + modifier);
        return modifier;
    }


    /// <summary>
    /// Calculates distance average
    /// </summary>
    /// <param name="_distancesReachedArr">average of all distances travelled</param>
    /// <returns>average distance</returns>
    private static float CalculateDistanceAverage(float[] _distancesReachedArr)
    {
        float new_distance_average = 0f;
        //Get average distance reached
        int counter = 0;
        for (int i = 0; i < _distancesReachedArr.Length; i++)
        {
            new_distance_average += _distancesReachedArr[i];
            counter++;
        }
        if (counter != 0)
        {
            new_distance_average /= counter;
        }
        Debug.Log("Distance average: " + new_distance_average);
        return new_distance_average;
    }

    /// <summary>
    /// Calculate player type with input data by average reached distance
    /// </summary>
    private static EPlayerSkillLevel CalculatePlayerSkillLevel(float _distanceReachedAverage, EPlayerSkillLevel[] _playerSkillLevelArr)
    {
        if (_distanceReachedAverage < c_beginnerThreshold)
        {
            return EPlayerSkillLevel.NOOB;
        }
        else if (_distanceReachedAverage < c_intermediateThreshold)
        {
            return EPlayerSkillLevel.BEGINNER;
        }
        else if (_distanceReachedAverage < c_advancedThreshold)
        {
            return EPlayerSkillLevel.INTERMEDIATE;
        }
        else if (_distanceReachedAverage < c_expertThreshold)
        {
            return EPlayerSkillLevel.ADVANCED;
        }
        else return EPlayerSkillLevel.EXPERT;
    }

    /// <summary>
    /// This function has to be called before starting a new game to calculate 
    /// the current playertype, so it can be used to update the difficulty accordingly
    /// If probability is negative -> return EPlayerType.EASY_FUN else return EPlayerType.HARD_FUN
    /// </summary>
    private static EPlayerType CalculatePlayerType(float _distanceReachedAverage, int[] _deathCounterArr, EPlayerType[] _playerTypeArr, EPlayerSkillLevel[] _playerSkillArr, int[] _timesLaunched)
    {
        EPlayerType new_player_type = EPlayerType.NONE;
        int probabilty = 0;

        //Latest player type
        EPlayerType last_player_type = _playerTypeArr[_playerTypeArr.Length-1];

        //Latest skill level
        EPlayerSkillLevel last_player_skill_level = _playerSkillArr[_playerSkillArr.Length-1];
        //Earlier value to compare to if it exists | assigns NONE if index is invalid
        EPlayerSkillLevel third_last_player_skill_level = (_playerSkillArr.Length - 4 >= 0 ? _playerSkillArr[_playerSkillArr.Length - 4] : EPlayerSkillLevel.NONE);

        //Latest counter of how many times the game was launched
        int last_times_launched_counter = _timesLaunched[_timesLaunched.Length - 1];

        //Earlier value to compare to if it exists | assigns -1 if index is invalid
        int fifth_last_times_launched_counter = _timesLaunched.Length - 6 >= 0 ? _timesLaunched[_timesLaunched.Length - 6] : -1;

        int last_death_counter = _deathCounterArr[_deathCounterArr.Length - 1];

        //Increases probability -> higher chance for HARD_FUN 
        //Decreases probability -> higher chance for EASY_FUN 

        //Chance to keep the previously calculated player type
        if (last_player_type == EPlayerType.EASY_FUN)
        {
            probabilty += Weights.c_weight30;
        } else if (last_player_type == EPlayerType.HARD_FUN)
        {
            probabilty -= Weights.c_weight30;
        }
        Debug.Log("Probability after step 1: " + probabilty);

        //Increase probability for freshest calculated skill level if HIGHER than INTERMEDIATE
        //Decrease probability for freshest calculated skill level if LOWER than INTERMEDIATE
        //Add nothing if freshest calculated skill level was INTERMEDIATE
        probabilty += ((int)last_player_skill_level - 2) * Weights.c_weight10;
        Debug.Log("Probability after step 2: " + probabilty);

        //Validity check
        if (third_last_player_skill_level != EPlayerSkillLevel.NONE)
        {
            //Same as previous step, with an EARLIER calculated skill level (if possible) and lower weights
            probabilty += ((int)third_last_player_skill_level - 2) * Weights.c_weight5;
        }
        Debug.Log("Probability after step 3: " + probabilty);

        //Increase probability if _distanceReachedAverage is BIGGER or EQUAL to c_intermediateThreshold
        //Decrease probability if _distanceReachedAverage is SMALLER than c_intermediateThreshold
        probabilty += (_distanceReachedAverage >= c_intermediateThreshold ? 1 : -1) * Weights.c_weight10;
        Debug.Log("Probability after step 4: " + probabilty);

        //Validity check
        if (fifth_last_times_launched_counter != -1)
        {    
            //Increase probability if same game session within x games
            //Decrease probability if different game session within x games
            probabilty += (last_times_launched_counter == fifth_last_times_launched_counter ? 1 : -1) * Weights.c_weight20;
        }
        Debug.Log("Probability after step 5: " + probabilty);

        //Calculate player type according to probability, if <= 0 -> EASY_FUN, else HARD_FUN
        new_player_type = (probabilty <= 0) ? (new_player_type = EPlayerType.EASY_FUN) : (new_player_type = EPlayerType.HARD_FUN);
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

/// <summary>
/// different weight values to increase the bias towards one or another outcome
/// </summary>
public static class Weights
{
    public const int c_weight5 = 5;
    public const int c_weight10 = 10;
    public const int c_weight20 = 20;
    public const int c_weight30 = 30;
}

