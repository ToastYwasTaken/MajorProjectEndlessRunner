using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;
using Unity.VisualScripting.Antlr3.Runtime;
/******************************************************************************
* Project: MajorProjectEndlessRunner
* File: ProceduralGeneration.cs
* Version: 1.0
* Autor: Franz M?rike (FM)
* 
* These coded instructions, statements, and computer programs contain
* proprietary information of the author and are protected by Federal
* copyright law. They may not be disclosed to third parties or copied
* or duplicated in any form, in whole or in part, without the prior
* written consent of the author.
* 
* ChangeLog
* ----------------------------
*  15.11.2023   FM  created, implemented spawning behaviour for Templates, 
*                   including offsets to increase the size of the templates according to the players speed increase
*  16.11.2023   FM  attempted fixing template spawn location
*  17.11.2023   FM   Fixed template spawn location. the next template now always spawns at the 
*                   end of the last one and increases it's size
*  20.11.2023   FM  added scaling for invisible walls, fixed render distance
*  21.11.2023   FM  added null check, added UpdatePrefabsOnGameStateChange(), added SpawnRandomObstacles(), added subscribing to event
<<<<<<< Updated upstream
*                   
*  TODO:            still have to fix the scripts to work as intended
*                   Add randomization for ground size
*                   make c_ not c_
=======
*  22.11.2023   FM  renamed UpdatePrefabsOnGameStateChange() to UpdateTemplates(), added color determination and assignment in UpdateTemplates()
*                  
*  TODO:
*       - Implement randomized factor for spawning templates
>>>>>>> Stashed changes
*  
*****************************************************************************/
public class ProceduralGeneration : MonoBehaviour 
{
    #region GameController Variables
    [SerializeField]
    private GameObject gameModeControllerGO;
    private GameModeController m_gameModeControllerRef;
    private GameModes m_currentGameMode;
    #endregion

    #region Player Variables
    [SerializeField]
    private GameObject playerGO;
    private float m_playerPositionZ;
    private float m_speedModifier = 1f;
    private float m_spawnPositionOffsetZ = 0f;
    #endregion

    #region Template Variables
    [SerializeField]
    private GameObject templatePrefabGO;
    [SerializeField, Range(50f, 2000f)]
    private float renderDistance;
    private List<GameObject> m_templateList = new List<GameObject>();
    private int m_templateCounter = 0;
    private Vector3 m_templateSpawnPosition;
    private Quaternion m_templateSpawnRotation;
<<<<<<< Updated upstream
    private float m_spawnPositionOffsetZ = 0f;
    private float m_groundSizeIncrease = 5f;
    private Vector3 m_wallScale;    //stores wallScale values
    private float c_originalWallScaleX = 1f;
    private float c_originalWallScaleY = 1f;
    private float c_originalWallScaleZ = 30f;
    private Vector3 m_groundScale;  //stores groundScale values
    private float c_originalGroundScaleX = 10f;
    private float c_originalGroundScaleY = 1f;
    private float c_originalGroundScaleZ = 30f;
=======
    //Wall Variables
    private Vector3 m_wallScale;    
    private const float c_originalWallScaleX = 1f;
    private const float c_originalWallScaleY = 1f;
    private const float c_originalWallScaleZ = 30f;
    private Color32 m_wallColor;
    //Ground Variables
    private Vector3 m_groundScale;  
    private const float c_originalGroundScaleX = 10f;
    private const float c_originalGroundScaleY = 1f;
    private const float c_originalGroundScaleZ = 30f;
    private float m_groundSizeIncrease = 5f;
    private Color32 m_groundColor;
    #endregion
>>>>>>> Stashed changes

    #region Obstacle Variables
    [SerializeField, Range(0.1f,1f)]
    private float obstacleDensity;
    private GameObject[] m_obstaclePrefabsGOArr;
    private Vector3 m_obstacleSpawnPosMin;
    private Vector3 m_obstacleSpawnPosMax;
    private float m_obstacleX;
    private float m_obstacleZ;
    private Color32 m_obstacleColor;
    #endregion

    private void Awake()
    {
        if (playerGO == null)
        {
            playerGO = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
        }
        if(gameModeControllerGO == null)
        {
            gameModeControllerGO = GameObject.FindObjectOfType<GameModeController>().gameObject;
            m_gameModeControllerRef = gameModeControllerGO.GetComponent<GameModeController>();
            m_currentGameMode = m_gameModeControllerRef.GetCurrentGameMode();
        }
        //Load all available obstacles from resources
        m_obstaclePrefabsGOArr = Resources.LoadAll<GameObject>("OBSTACLES");
        m_obstacleColor = new Color(0, 0, 0, 255);
    }
    private void Start()
    {
        SetUpInitialTemplate();
        SpawnTemplate(templatePrefabGO, m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale);
    }

    private void Update()
    {
        //Update player pos
        m_playerPositionZ = playerGO.transform.position.z;
        //Check if game mode updated
        GameModeController.OnGameModeUpdated += UpdateTemplates;
        GameModeController.OnGameModeUpdated -= UpdateTemplates;
        //Debug.Log("tempSpawnPosZ: " + (m_templateSpawnPosition.z) + "playerposZ: " + m_playerPositionZ + "render dis: " + renderDistance);
        //Generate new Templates when template pos - player position reaches render distance
        if (m_templateSpawnPosition.z - m_playerPositionZ < renderDistance)
        {
            CalculateNextTemplateValues();
            //Spawn next template
            SpawnTemplate(templatePrefabGO, m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale);
            //Spawn obstacles
            SpawnRandomObstacles();
        }
    }

    /// <summary>
    /// Change colors of prefab on difficulty change
    /// </summary>
    private void UpdateTemplates()
    {
        Debug.Log("In UpdateTemplates()");
        //Set colors according to game mode
        //They stay the same for START and VERY_EASY
        if (m_currentGameMode == GameModes.EASY)
        {
            m_groundColor = new Color32(0, 255, 250, 255);
            m_wallColor = new Color32(10, 125, 120, 255);
        }
        else if (m_currentGameMode == GameModes.MEDIUM)
        {
            m_groundColor = new Color32(10, 25, 240, 255);
            m_wallColor = new Color32(10, 15, 105, 255);
        }
        else if (m_currentGameMode == GameModes.HARD)
        {
            m_groundColor = new Color32(255, 155, 0, 255);
            m_wallColor = new Color32(155, 95, 10, 255);
        }
        else if(m_currentGameMode == GameModes.VERY_HARD)
        {
            m_groundColor = new Color32(255, 0, 0, 255);
            m_wallColor = new Color32(115, 5, 5, 255);
        }
        else if(m_currentGameMode == GameModes.EXTREME)
        {
            m_groundColor = new Color32(255, 0, 255, 255);
            m_wallColor = new Color32(150, 15, 150, 255);
        }
        //Update Colors
        templatePrefabGO.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Renderer>().material.color = m_groundColor;
        templatePrefabGO.transform.GetChild(1).GetChild(0).gameObject.GetComponent<Renderer>().material.color = m_wallColor;
        templatePrefabGO.transform.GetChild(1).GetChild(2).gameObject.GetComponent<Renderer>().material.color = m_wallColor;
    }

    /// <summary>
    /// Setting up the starting template and instantiating it
    /// </summary>
    private void SetUpInitialTemplate()
    {
        m_groundScale = new Vector3(c_originalGroundScaleX, c_originalGroundScaleY, c_originalGroundScaleZ);
        m_wallScale = new Vector3(c_originalWallScaleX, c_originalWallScaleY, c_originalWallScaleZ);
        m_spawnPositionOffsetZ =  m_groundScale.z / 2;
        m_templateSpawnPosition = playerGO.transform.position + new Vector3(0, -1.5f, m_spawnPositionOffsetZ);  //-1.5f is the height of player spawn offset
        m_templateSpawnRotation = Quaternion.identity;
    }

    /// <summary>
    /// Updating m_groundScale.z, m_wallScale.z and calculating m_templateSpawnPosition 
    /// Incrementing future platform size and templateCounter
    /// </summary>
    private void CalculateNextTemplateValues()
    {
        //Increasing platform size for next platform
        m_groundScale.z += m_groundSizeIncrease;
        m_wallScale.z += m_groundSizeIncrease;
        //Update templateSpawnPos according to the size increase of the next spawned platform
        float templateSpawnPosZ = m_templateList[m_templateCounter].transform.GetChild(0).GetChild(0).localScale.z ;
        m_templateSpawnPosition += new Vector3 (0,0,templateSpawnPosZ + (m_groundSizeIncrease/2));
        //Increasing the increase for future spawns to counter shorter sections in higher speeds
        m_groundSizeIncrease += 2;
        m_templateCounter++;
    }

    /// <summary>
    /// Spawning a template at given position, rotation and assigning the templates childs ground and wall prefab scales
    /// Calling UpdatePlayerSpeedModifier() to decrease player speed increase
    /// </summary>
    /// <param name="_templateToSpawn">Template that is spawned</param>
    /// <param name="_templateSpawnPosition">Template spawn position</param>
    /// <param name="_templateSpawnRotation">Template spawn rotation</param>
    /// <param name="_groundScaleVector">Child (GROUND) of Child (Ground) localscale</param>
    /// <param name="_wallScaleVector">Child (WALL) of Child (Wall) localscale</param>
    private void SpawnTemplate(GameObject _templateToSpawn, Vector3 _templateSpawnPosition, Quaternion _templateSpawnRotation, Vector3 _groundScaleVector, Vector3 _wallScaleVector)
    {
        GameObject template = Instantiate(_templateToSpawn, _templateSpawnPosition, _templateSpawnRotation);
        //Update the prefabs scales
        //Update ground scale and wall scale
        template.transform.GetChild(0).GetChild(0).localScale = _groundScaleVector;
        //right lower wall
        template.transform.GetChild(1).GetChild(0).localScale = _wallScaleVector;
        //left lower wall
        template.transform.GetChild(1).GetChild(2).localScale = _wallScaleVector;
        //Update wallScaleVector for upper walls
        _wallScaleVector = new Vector3(_wallScaleVector.x, 7f, _wallScaleVector.z);
        //right upper wall
        template.transform.GetChild(1).GetChild(1).localScale = _wallScaleVector;
        //left upper wall
        template.transform.GetChild(1).GetChild(3).localScale = _wallScaleVector;
        //Adding spawned template to list
        m_templateList.Add(template);
        //Updating player speed to increase at an reduced rate
        UpdatePlayerSpeedModifier();
        Debug.Log("Spawned Template nr."+ m_templateList.Count +" at: " + _templateSpawnPosition);
    }

    private void SpawnRandomObstacles()
    {
        System.Random rdm = new System.Random();
        //Choose amount of obstacles depending on size of ground/template
        int maxAmountOfPrefabsToSpawn = (int) (m_groundScale.z * obstacleDensity);
        int randomAmountOfPrefabsToSpawn = rdm.Next(0, maxAmountOfPrefabsToSpawn + 1);
        for (int i = 0; i < randomAmountOfPrefabsToSpawn; i++)
        {
            //Choose random obstacle
            int randomPrefabNr = rdm.Next(0, m_obstaclePrefabsGOArr.Length+1);
            //Calculate spawn area TODO
            m_obstacleSpawnPosMin = new Vector3(0,0,m_templateSpawnPosition.z);
            m_obstacleSpawnPosMax = new Vector3();
            //Randomize spawn positions, trying not to overlap

            //Assign color
            m_obstaclePrefabsGOArr[i].GetComponent<Renderer>().material.color = m_obstacleColor;
        }
    }

    /// <summary>
    /// This updates the playerspeed to accellerate slower so the speed doesn't get too fast that quick
    /// It's updated whenever new Templates are spawned
    /// </summary>
    private void UpdatePlayerSpeedModifier()
    {
        m_speedModifier -= 0.05f;
    }
    public float GetSpeedModifier()
    {
        return m_speedModifier;
    }
}
