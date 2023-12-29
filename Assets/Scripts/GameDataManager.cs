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
 * File: GameDataManager.cs
 * Version: 1.0
 * Autor: Franz M?rike (FM)
 * 
 * These coded instructions, statements, and computer programs contain
 * proprietary information of the author and are protected by Federal
 * copyright law. They may not be disclosed to third parties or copied
 * or duplicated in any form, in whole or in part, without the prior
 * written consent of the author.
 * ----------------------------
 * DISCLAIMER: PARTS of the implemented saving system source from: Unity Game Development Cookbook
 *             and were NOT coded by myself.
 * (Link: https://learning.oreilly.com/library/view/unity-game-development/9781491999141/ch02.html#scripting-persistent-data-path)
 * ----------------------------
 * Script Description:
 * Saving and loading of game variables in a .json file
 * 
 * ----------------------------
 * ChangeLog:
 *  18.12.2023   FM  created
 *  28.12.2023   FM  added id to save file, added subFolder segregation between global and session files
 *  
 *  TODO: 
 *      - 
 *      
 *  Buglist:
 *      - 
 *  
 *****************************************************************************/
public static class GameDataManager
{
    private const string c_objectsKey = "objects";
    private const string c_saveID = "$saveID";
    private const string c_playerSkillLevelKey = "playerSkillKey";
    private const string c_playerTypeKey = "playerTypeKey";
    private const string c_globalSaveFolder = "/GlobalSaves";
    private const string c_sessionSaveFolder = "/SessionSaves";
    static UnityAction<Scene, LoadSceneMode>
    ALoadObjectsAfterSceneLoad;
    public static int S_sessionID { get {return s_sessionID; } private set {s_sessionID = value; } }
    private static int s_sessionID = 0;

    ///// <summary>
    ///// Combines all data to global .json file
    ///// </summary>
    ///// <param name="_fileName">name of file to save</param>
    //public static void SaveGlobalStats(string _fileName)
    //{
    //    var sub_folder = c_globalSaveFolder ;
    //    Debug.Log("Saving directory: "+ Path.Combine(Application.persistentDataPath + sub_folder));
    //    //Create directory if not already existing
    //    if (!Directory.Exists(Path.Combine(Application.persistentDataPath + sub_folder)))
    //    {
    //        Debug.Log("Creating new dir");
    //        Directory.CreateDirectory(Path.Combine(Application.persistentDataPath + sub_folder));
    //    }
    //    //Create new output path
    //    string output_path;
    //    JsonData final_json_data = new JsonData();
    //    output_path = Path.Combine(Application.persistentDataPath + sub_folder, Path.GetFileNameWithoutExtension(_fileName) + ".json");

    //    //create global save file from session files
    //    var files_in_session_folder = Directory.GetFiles(Path.Combine(Application.persistentDataPath, c_sessionSaveFolder));
    //    for(int i = 0; i < files_in_session_folder.Length; i++)
    //    {
    //        var file_name = "SessionSave" + i + ".json";
    //        string text = File.ReadAllText(Path.Combine(Application.persistentDataPath + c_sessionSaveFolder, file_name));

    //    } 
    //    //Write json file
    //    var json_writer = new JsonWriter();
    //    json_writer.PrettyPrint = true;
    //    final_json_data.ToJson(json_writer);
    //    //Save file
    //    File.WriteAllText(output_path, json_writer.ToString());
    //    Debug.Log("Player stats saved successfully at: " + output_path);
    //    //Run GC to clear alloc memory
    //    final_json_data = null;
    //    System.GC.Collect();
    //}

    /// <summary>
    /// Saves session data to .json file
    /// </summary>
    /// <param name="_fileName">name of file to save</param>
    public static void SaveSessionStats(string _fileName)
    {
        var sub_folder = c_sessionSaveFolder;
        s_sessionID = Directory.GetFiles(Path.Combine(Application.persistentDataPath + c_sessionSaveFolder)).Length;
        Debug.Log("Saving directory: " + Path.Combine(Application.persistentDataPath + sub_folder));
        //Create directory if not already existing
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath + sub_folder)))
        {
            Debug.Log("Creating new dir");
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath + sub_folder));
        }
        //Create new output path
        string output_path;
        JsonData final_json_data = new JsonData();
        output_path = Path.Combine(Application.persistentDataPath + sub_folder, Path.GetFileNameWithoutExtension(_fileName) + s_sessionID + ".json");

        //locate all classes implementing ISaveable
        var all_saveable_objects = Object.FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        //Save data
        if (all_saveable_objects.Count() > 0)
        {
            JsonData saved_objects = new JsonData();
            foreach (var saveable_object in all_saveable_objects)
            {
                var data = saveable_object.SavedData;
                data[c_saveID] = saveable_object.SaveID;
                saved_objects.Add(data);
            }
            final_json_data[c_objectsKey] = saved_objects;
        }
        //Write json file
        var json_writer = new JsonWriter();
        json_writer.PrettyPrint = true;
        final_json_data.ToJson(json_writer);
        //Save file
        File.WriteAllText(output_path, json_writer.ToString());
        Debug.Log("Player stats saved successfully at: " + output_path);
        //Run GC to clear alloc memory
        final_json_data = null;
        System.GC.Collect();
    }

    ///// <summary>
    ///// Loads previously saved data
    ///// </summary>
    ///// <param name="_fileName">name of file to load</param>
    ///// <returns>true if loaded successfully</returns>
    //public static bool LoadGlobalStats(string _fileName)
    //{
    //    string sub_folder = c_globalSaveFolder;
    //    string path;
    //    s_sessionID = Directory.GetFiles(Path.Combine(Application.persistentDataPath + c_sessionSaveFolder)).Length;
    //    path = Path.Combine(Application.persistentDataPath + sub_folder, _fileName);
    //    // path = Path.Combine(Application.persistentDataPath + sub_folder, Path.GetFileNameWithoutExtension(_fileName) + s_sessionID + ".json");

    //    Debug.Log("Trying to load player stats at: " + path);
    //    if (File.Exists(path))
    //    {
    //        string text = File.ReadAllText(path);
    //        JsonData data = JsonMapper.ToObject(text);
    //        if (data != null && data.IsObject == true)
    //        {
    //            if (data.ContainsKey(c_objectsKey))
    //            {
    //                //Get objects
    //                var objects = data[c_objectsKey];
    //                //Create delegate
    //                ALoadObjectsAfterSceneLoad = (scene, loadSceneMode) =>
    //                {
    //                    // Find all loadable objects implementing ISavable
    //                    var allLoadableObjects = Object
    //                        .FindObjectsOfType<MonoBehaviour>()
    //                        .OfType<ISaveable>()
    //                        .ToDictionary(o => o.SaveID, o => o);
    //                    // Get the collection of objects we need to load
    //                    var objectsCount = objects.Count;
    //                    for (int i = 0; i < objectsCount; i++)
    //                    {
    //                        // Get saved data of objects
    //                        var objectData = objects[i];
    //                        // Get the Save ID
    //                        var saveID = (string)objectData[c_saveID];
    //                        // Find loadable objects by saveID
    //                        if (allLoadableObjects.ContainsKey(saveID))
    //                        {
    //                            var loadableObject = allLoadableObjects[saveID];
    //                            //Load object data
    //                            //Debug.Log("Loading object data");
    //                            loadableObject.LoadFromData(objectData);
    //                        }
    //                    }
    //                    //Remove delegate
    //                    SceneManager.sceneLoaded -= ALoadObjectsAfterSceneLoad;
    //                    // Remove ref
    //                    ALoadObjectsAfterSceneLoad = null;
    //                    //Run GC to clear alloc memory
    //                    System.GC.Collect();
    //                };
    //                // Register the object-loading code to run after the scene loads.
    //                SceneManager.sceneLoaded += ALoadObjectsAfterSceneLoad;
    //            }
    //            Debug.Log("Player stats loaded successfully");
    //            return true;
    //        }
    //        else return false;
    //    }
    //    else return false;
    //}

    /// <summary>
    /// Loads previously saved data
    /// </summary>
    /// <param name="_fileName">name of file to load</param>
    /// <returns>true if loaded successfully</returns>
    public static bool LoadSessionStats(string _fileName)
    {
        string sub_folder = c_sessionSaveFolder;
        string path;
        s_sessionID = Directory.GetFiles(Path.Combine(Application.persistentDataPath + c_sessionSaveFolder)).Length;
        path = Path.Combine(Application.persistentDataPath + sub_folder, _fileName);
        // path = Path.Combine(Application.persistentDataPath + sub_folder, Path.GetFileNameWithoutExtension(_fileName) + s_sessionID + ".json");

        Debug.Log("Trying to load player stats at: " + path);
        if (File.Exists(path))
        {
            string text = File.ReadAllText(path);
            JsonData data = JsonMapper.ToObject(text);
            if (data != null && data.IsObject == true)
            {
                if (data.ContainsKey(c_objectsKey))
                {
                    //Get objects
                    var objects = data[c_objectsKey];
                    //Create delegate
                    ALoadObjectsAfterSceneLoad = (scene, loadSceneMode) =>
                    {
                        // Find all loadable objects implementing ISavable
                        var allLoadableObjects = Object
                            .FindObjectsOfType<MonoBehaviour>()
                            .OfType<ISaveable>()
                            .ToDictionary(o => o.SaveID, o => o);
                        // Get the collection of objects we need to load
                        var objectsCount = objects.Count;
                        for (int i = 0; i < objectsCount; i++)
                        {
                            // Get saved data of objects
                            var objectData = objects[i];
                            // Get the Save ID
                            var saveID = (string)objectData[c_saveID];
                            // Find loadable objects by saveID
                            if (allLoadableObjects.ContainsKey(saveID))
                            {
                                var loadableObject = allLoadableObjects[saveID];
                                //Load object data
                                //Debug.Log("Loading object data");
                                loadableObject.LoadFromData(objectData);
                            }
                        }
                        //Remove delegate
                        SceneManager.sceneLoaded -= ALoadObjectsAfterSceneLoad;
                        // Remove ref
                        ALoadObjectsAfterSceneLoad = null;
                        //Run GC to clear alloc memory
                        System.GC.Collect();
                    };
                    // Register the object-loading code to run after the scene loads.
                    SceneManager.sceneLoaded += ALoadObjectsAfterSceneLoad;
                }
                Debug.Log("Player stats loaded successfully");
                return true;
            }
            else return false;
        }
        else return false;
    }
}



/// <summary>
/// A savable variation of MonoBehaviour
/// The abstract class only assigns a new unique save ID
/// </summary>
public abstract class SaveableBehavior : MonoBehaviour, ISaveable, ISerializationCallbackReceiver 
{
    public abstract JsonData SavedData { get; }
    public abstract void LoadFromData(JsonData data);

    public string SaveID
    {
        get
        {
            return saveID;
        }
        set
        {
            saveID = value;
        }
    }

    [HideInInspector]
    [SerializeField]
    private string saveID;

    public void OnBeforeSerialize()
    {
        if (saveID == null)
        {
            saveID = System.Guid.NewGuid().ToString();
        }
    }

    public void OnAfterDeserialize()
    {
       
    }
}

/// <summary>
/// Implemented by SavableBehaviour
/// </summary>
internal interface ISaveable
{
    public string SaveID { get; }
    public JsonData SavedData { get; }
    public void LoadFromData(JsonData _data);
}
