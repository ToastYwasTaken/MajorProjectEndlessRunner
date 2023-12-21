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
 * 
 * ----------------------------
 * ChangeLog:
 *  20.12.2023   FM  created
 *  
 *  TODO: 
 *      - 
 *      
 *  Buglist:
 *      - 
 *  
 *****************************************************************************/
public static class DynamicDifficultyAdjuster
{
    public static float CalculateAdjustedSpeedModifier(float _oldSpeedModifier, float _distanceTravelled, int _deathCounter)
    {
        float 
        float new_speed_modifier = MapperHelper.RemapValueTwoRanges(_oldSpeedModifier, );

        return new_speed_modifier;
    }

    


}


/// <summary>
/// Summarises playertypes by skill level
/// </summary>
public enum EPlayerType
{
    NOOB = 0,
    BEGINNER = 1,
    INTERMEDIATE = 2,
    ADVANCED = 3,
    PRO = 4,
}