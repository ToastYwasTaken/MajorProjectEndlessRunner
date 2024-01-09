using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Events;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: SavingService.cs
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
 * Saving and loading of game and DDA variables 
 * 
 * ----------------------------
 * ChangeLog:
 *  29.12.2023   FM  created
 *  30.12.2023   FM  implemented SaveData() and LoadData() to load and save data with PlayerPrefs; removed CreateSaveKey()
 *  01.01.2023   FM  added timesLaunched
 *  09.01.2023   FM  adjusted s_maxSaveFiles, updated deletion of PlayerPrefs condition
 *  
 *  TODO: 
 *      - Save PCG values
 *      
 *  Buglist:
 *      - 
 *  
 *****************************************************************************/
public static class SavingService
{
    private static int s_saveID;
    private static int s_maxSaveFiles = 5000; //note that per save instance currently 5 values are save at a time, the max save space PlayerPrefs allowed is 1MB, which equals ~58250 files
    private static string s_distanceTravelledKey = "distance";
    private static string s_deathCounterKey = "deathCounter";
    private static string s_playerTypeKey = "playerType";
    private static string s_playerSkillLevelKey = "playerSkill";
    private static string s_timesLaunchedKey = "timesLaunched";

    /// <summary>
    /// Saves Data across game sessions
    /// </summary>
    /// <param name="_distanceTravelled">distance the player travelled before failing</param>
    /// <param name="_deathCounter">fail counter that increases each time the player dies</param>
    /// <param name="_playerType">by dda analysed player type</param>
    /// <param name="_playerSkillLevel">by dda analysed player skill level</param>
    /// <param name="_timesLaunched">times the game was launched over all time</param>
    public static void SaveData(float _distanceTravelled, int _deathCounter, EPlayerType _playerType, EPlayerSkillLevel _playerSkillLevel, int _timesLaunched)
    {
        string distance_key = s_distanceTravelledKey + s_saveID;
        //Check if key is already present
        while (PlayerPrefs.HasKey(distance_key))
        {
            s_saveID++;
            //Delete save files to avoid overflow | the number represents the amount of save files per save id
            if(s_saveID * 5 >= s_maxSaveFiles)
            {
                Debug.Log("Deleting save files");
                PlayerPrefs.DeleteAll();
                //resetting saveID
                s_saveID = 0;
            }
            distance_key = s_distanceTravelledKey + s_saveID;
        }
        //Create save keys
        string death_counter_key = s_deathCounterKey + s_saveID;
        string player_type_key = s_playerTypeKey + s_saveID;
        string player_skill_level_key = s_playerSkillLevelKey + s_saveID;
        string times_launched_key = s_timesLaunchedKey + s_saveID;
        //Save values / cast enums
        PlayerPrefs.SetFloat(distance_key, _distanceTravelled);
        PlayerPrefs.SetInt(death_counter_key, _deathCounter);
        PlayerPrefs.SetInt(player_type_key, (int)_playerType);
        PlayerPrefs.SetInt(player_skill_level_key, (int)_playerSkillLevel);
        PlayerPrefs.SetInt(times_launched_key, _timesLaunched);
        PlayerPrefs.Save();
        //Debug.Log("Saved distance [Key]: " + distance_key + " - [Value]: " + _distanceTravelled + 
        //    " and deathCounter [Key]: " + death_counter_key + " - [Value]: " + _deathCounter +
        //    " and playerType [Key]: " + player_type_key + " - [Value]: " + _playerType +
        //    " and playerSkillLevel [Key]: " + player_skill_level_key + " - [Value]: " + _playerSkillLevel);
    }

    /// <summary>
    /// Load all saved data 
    /// Even though you could save space by only saving the new values e.g. for deathCounter which just increments,
    /// I decided to keep it in arrays to get all information recorded at each run
    /// </summary>
    /// <returns>arrays containing all saved values. </returns>
    public static (float[], int[], EPlayerType[], EPlayerSkillLevel[], int[]) LoadData()
    {
        string distance_key = "";
        string death_counter_key = "";
        string player_type_key = "";
        string player_skill_level_key = "";
        string times_launched_key = "";
        int arr_length = s_saveID + 1;
        float[] distance_arr = new float[arr_length];
        int[] death_counter_arr = new int[arr_length];
        EPlayerType[] player_type_arr = new EPlayerType[arr_length];
        EPlayerSkillLevel[] player_skill_level_arr = new EPlayerSkillLevel[arr_length];
        int[] times_launched_arr = new int[arr_length];
        //If any data was saved: load it
        for (int i = 0; i < s_saveID + 1; i++)
        {
            distance_key = (s_distanceTravelledKey + i);
            death_counter_key = (s_deathCounterKey + i);
            player_type_key = (s_playerTypeKey + i);
            player_skill_level_key = (s_playerSkillLevelKey + i);
            distance_arr[i] = PlayerPrefs.GetFloat(distance_key);
            death_counter_arr[i] = PlayerPrefs.GetInt(death_counter_key);
            player_type_arr[i] = (EPlayerType)PlayerPrefs.GetInt(player_type_key);
            player_skill_level_arr[i] = (EPlayerSkillLevel)PlayerPrefs.GetInt(player_skill_level_key);
            times_launched_arr[i] = PlayerPrefs.GetInt(times_launched_key);
            //Debug.Log("Loading distance [Key]: " + distance_key + " - [Value]: " + distance_arr[i] +
            //        " and deathCounter [Key]: " + death_counter_key + " - [Value]: " + death_counter_arr[i] +
            //        " and playerType [Key]: " + player_type_key + " - [Value]: " + player_type_arr[i] +
            //        " and playerSkillLevel [Key]: " + player_skill_level_key + " - [Value]: " + player_skill_level_arr[i]);
        }
        return (distance_arr, death_counter_arr, player_type_arr, player_skill_level_arr, times_launched_arr);
    }


}

