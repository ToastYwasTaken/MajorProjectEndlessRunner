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
 * Implements helper functions for value mapping
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
public static class MapperHelper 
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
    /// returns a remapped, smaller range to gain more controll over the randomization by narrowing the desired target range
    /// min is set fixed to 1 so it always spawns AT LEAST 1 obstacle (besides on starting template)
    /// </summary>
    /// <param name="_min">unmapped min value</param>
    /// <param name="_max">unmapped max value</param>
    /// <param name="_minRequiredRangeToMap">minimal range required to start remapping values</param>
    /// <returns>mapped new min and max value</returns>
    public static (int, int) RemapRange(int _min, int _max, float _factor, int _minRequiredRangeToMap)
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
}
