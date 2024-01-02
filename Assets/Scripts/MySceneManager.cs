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
* Also responsible for calling Save and Load functions at proper time
* 
* ----------------------------
* ChangeLog:
*  05.12.2023   FM  created; added Load functions for different scenes
*  18.12.2023   FM  added GameDataManager references to save and load data
*  28.12.2023   FM  added more saving / loading functionality
*  29.12.2023   FM  edited saving / loading
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
    private GameObject m_playerGO;
    private PlayerController m_playerRef;
    private int m_timesLaunched;
    private bool m_launchedGame = true;
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
        m_timesLaunched = DynamicDifficultyAdjuster.TimesLaunched;
        if (m_launchedGame)
        {
            m_timesLaunched++;
        }
        
    }

    public void LoadPlayMode()
    {
        DynamicDifficultyAdjuster.UpdateDifficulty();
        SceneManager.LoadScene("PlayMode");
    }
    public void LoadGameOver()
    {
        m_playerGO = GameObject.FindGameObjectWithTag("Player");
        m_playerRef = m_playerGO.GetComponent<PlayerController>();
        SavingService.SaveData(m_playerRef.PlayerPosition.z, m_playerRef.DeathCounter, m_playerRef.PlayerType , m_playerRef.PlayerSkillLevel, m_timesLaunched);
        SceneManager.LoadScene("GameOver");
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void DeleteDDAData()
    {
        PlayerPrefs.DeleteAll();
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    //Final call before Quitting
    private void OnApplicationQuit()
    {

    }
}
