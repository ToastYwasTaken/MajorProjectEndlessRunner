using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: MapperHelper.cs
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
 * Implements helper functions for value mapping (mostly used by ProceduralGeneration.cs)
 * 
 * ----------------------------
 * ChangeLog:
 *  21.12.2023   FM  created, mover RemapRange() here from ProceduralGeneration.cs,
 *                   added RemapValueFromOldRangeToNewRange
 *  
 *  TODO: 
 *      - 
 *      
 *  Buglist:
 *      - 
 *  
 *****************************************************************************/
public static class MapperAndRdmHelper 
{
    /// <summary>
    /// Remaps a value from an old range onto a new range
    /// </summary>
    /// <param name="value">value to remap</param>
    /// <param name="_oldMin">old range min value</param>
    /// <param name="_oldMax">old range max value</param>
    /// <param name="_newMin">new range min value</param>
    /// <param name="_newMax">new range max value</param>
    /// <returns>the remapped value in new range</returns>
    public static float RemapValueFromOldRangeToNewRange(float value, float _oldMin, float _oldMax, float _newMin, float _newMax)
    {
        double slope = (_newMax - _newMin) / (_oldMax - _oldMin);
        return (int)((value - _oldMin) * slope + _newMin);
    }
    
    /// <summary>
    /// returns a remapped range by a factor to gain more controll over the randomization by narrowing the desired target range
    /// min is set fixed to 1 so it always spawns AT LEAST 1 obstacle (besides on starting template
    /// </summary>
    /// <param name="_min">unmapped min value</param>
    /// <param name="_max">unmapped max value</param>
    /// <param name="_minRequiredRangeToMap">minimal range required to start remapping values</param>
    /// <returns>mapped new min and max value</returns>
    public static (int, int) ReduceRangeByFactor(int _min, int _max, float _factor, int _minRequiredRangeToMap)
    {
        int difference_min_max = _max - _min;
        if (difference_min_max >= _minRequiredRangeToMap)
        {
            int scaled_factor = (int)(difference_min_max * _factor);
            int mapped_min = _min + scaled_factor;
            int mapped_max = _max - scaled_factor;
            return (mapped_min, mapped_max);
        }
        else return (_min, _max);
    }

    /// <summary>
    /// Remaps a Range by a factor
    /// </summary>
    /// <param name="_min">min value of input range</param>
    /// <param name="_max">max value of input range</param>
    /// <param name="_factor">factor applied on the range</param>
    /// <returns>new range</returns>
    public static (int, int) RemapRangeByFactor(int _min, int _max, float _factor)
    {
        return ((int)(_min*_factor), (int)(_max*_factor));
    }

    /// <summary>
    /// Calculates a randomized Quaternion
    /// </summary>
    /// <returns>randomized Quaternion</returns>
    public static Quaternion RandomizeQuaternionY()
    {
        System.Random rdm = new System.Random();
        int random_y = 0;
        //Chance 2 : 1 to keep original Quaternion
        if (rdm.Next(0, 3) != 0)
        {
            random_y = rdm.Next(0, 360);
        }
        Quaternion randomized_quaternion = Quaternion.Euler(0, random_y, 0);
        return randomized_quaternion;
    }

    /// <summary>
    /// Choose amount of obstacles depending on size of ground/template and obstacleDensity
    /// </summary>
    /// <param name="_defaultValue">min value of obstacles that should be spawned</param>
    /// <param name="_currentGroundScaleZ">scale z of template</param>
    /// <param name="_originalGroundScaleZ">original scale z of template</param>
    /// <param name="_obstacleDensity">integrated to adjust the density</param>
    /// <param name="_currentGameMode">current game mode</param>
    /// <returns>amount of max prefabs to spawn</returns>
    public static int CalculateObstacleMaxSpawnAmount(int _defaultValue, float _currentGroundScaleZ, float _originalGroundScaleZ, float _obstacleDensity, EGameModes _currentGameMode)
    {
        int prefab_amount_max = _defaultValue + ((int)((_currentGroundScaleZ - _originalGroundScaleZ)
            * _obstacleDensity * (1 + (int)_currentGameMode) * 0.1f));
        return prefab_amount_max;
    }

    /// <summary>
    /// returns a random float within given range
    /// </summary>
    /// <param name="_minValue">inclusive min value</param>
    /// <param name="_maxValue">exclusive max value</param>
    /// <returns>a randomized float</returns>
    public static float RandomizeFloat(float _minValue, float _maxValue)
    {
        System.Random rdm = new System.Random();
        float value = (float)(rdm.NextDouble() * (_maxValue - _minValue) + _minValue);
        return value;
    }

}
