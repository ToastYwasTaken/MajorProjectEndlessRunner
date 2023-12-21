using UnityEngine;
using UnityEngine.SceneManagement;
/******************************************************************************
* Project: MajorProjectEndlessRunner
* File: MySceneManager.cs
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
* Manages switching game scenes when triggered by certain events (buttons in menus, death in playmode...)
* 
* ----------------------------
* ChangeLog:
*  05.12.2023   FM  created; added Load functions for different scenes
*  18.12.2023   FM  added GameDataManager references to save and load data
*  
*  TODO: 
*      - 
*  Buglist:
*      - 
*      
*****************************************************************************/
public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Instance { get; private set; }
    private void Awake()
    {
        //Singleton checks
        if (Instance == null && Instance != this)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
        }
    }

    public void LoadPlayMode()
    {
        GameDataManager.LoadStats("SaveFile.json");
        SceneManager.LoadScene("PlayMode");
    }
    public void LoadGameOver()
    {
        GameDataManager.SaveStats("SaveFile.json");
        SceneManager.LoadScene("GameOver");
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
