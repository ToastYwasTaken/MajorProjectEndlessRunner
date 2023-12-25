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
 * NOTE: This works per SESSION and does NOT save data ACCROSS sessions so far
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
    private static EPlayerType m_playerType = EPlayerType.NOOB;
    public static float CalculateAdjustedSpeedModifier(float _oldSpeedModifier)
    {
        float default_factor = (int)m_playerType;
        float new_speed_modifier = _oldSpeedModifier + default_factor;

        return new_speed_modifier;
    }

    public static EPlayerType UpdatePlayerType()
    {
        EPlayerType new_player_type;
        //Calculate player type with input data


        return new_player_type;
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