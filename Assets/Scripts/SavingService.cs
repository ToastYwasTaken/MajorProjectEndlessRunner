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
 * Saving and loading of game variables 
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
public static class SavingService
{
    private static string s_distanceTravelledKey = "distance";
    private static string s_deathCounterKey = "deathCounter";
    private static int s_id;
    private static float[] s_savedDistances;
    private static int[] s_savedDeathCounters;

    public static void SaveSessionData(float _distanceTravelled, int _deathCounter)
    {
        string distance_key = s_distanceTravelledKey + s_id.ToString();
        string death_counter_key = s_deathCounterKey + s_id.ToString();
        while (PlayerPrefs.HasKey(distance_key)|| PlayerPrefs.HasKey(death_counter_key))
        {
            distance_key = s_distanceTravelledKey + s_id++;
            death_counter_key = s_deathCounterKey + s_id;
            Debug.Log("Saving distance [Key]: " + distance_key + " - [Value]: " + _distanceTravelled + " and deathCounter [Key]: " + death_counter_key + " - [Value]: " + _deathCounter);
        }
        PlayerPrefs.SetFloat(distance_key, _distanceTravelled);
        PlayerPrefs.SetInt(death_counter_key, _deathCounter);
    }

    public static (float[], int[]) LoadSessionData()
    {
        string distance_key = "";
        string death_counter_key = "";
        float[] distance_arr = new float[s_id];
        int[] death_counter_arr = new int[s_id];
        for (int i = 0; i < s_id + 1; i++)
        {
            distance_key = i.ToString();
            death_counter_key = i.ToString();
            distance_arr[i] = PlayerPrefs.GetFloat(distance_key);
            death_counter_arr[i] = PlayerPrefs.GetInt(death_counter_key);
            Debug.Log("Loading distance [Key]: " + distance_key + " - [Value]: " + distance_arr[i] + " and deathCounter [Key]: " + death_counter_arr[i] + " - [Value]: " + _deathCounter);
        }
        return (distance_arr, death_counter_arr);
    }

    public static void DeleteSessionData()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void SaveGlobalData()
    {

    }

    public static sbyt LoadGlobalData()
    {

    }
}
