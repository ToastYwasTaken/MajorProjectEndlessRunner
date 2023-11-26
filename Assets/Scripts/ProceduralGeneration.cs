using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine.Android;
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
*  22.11.2023   FM  renamed UpdatePrefabsOnGameStateChange() to UpdateTemplates(), added color determination and assignment in UpdateTemplates(),
*                   resolved merge conflict, implemented randomized factor for spawning templates
*  24.11.2023   FM  resolved bug, which denied accessing updated variables from player since the prefab was referenced instead of the playerGO in scene.
*                   removed speed modifier reference for PlayerController -> will be handled completely in player; added tooltips
*  26.11.2023   FM  fixed GameMode changing to work accordingly and adjust the colors of the spawned prefabs as wanted, added SpawnBehaviour for random Obstacles
*  
*  TODO:
*       - implement obstacle spawning
*       - tweak constraints for Obstacle Spawning
*  Buglist:
*       - resolve accessing variables from other GO not working - resolved
*  
*****************************************************************************/
public class ProceduralGeneration : MonoBehaviour 
{
    #region GameController Variables
    [SerializeField, Tooltip("GameModeController GameObject in game scene, gets assigned in Awake() if unassigned")]
    private GameObject gameModeControllerGO;
    private GameModeController m_gameModeControllerRef;
    private GameModes m_currentGameMode;
    #endregion

    #region Player Variables
    [SerializeField, Tooltip("Player GameObject in active game scene, gets assigned in Awake() if unassigned")]
    private GameObject playerGO; //Assign player here | Note: DON'T assign the Prefab from Prefabs folder, instead use hierachy player; if no player is assigned, it is found in Awake();
    private PlayerController m_playerControllerRef;
    private float m_playerPositionZ;
    private float m_spawnPositionOffsetZ = 0f;
    #endregion

    #region Template Variables
    [SerializeField, Tooltip("Prefab of basic template to be instantiated")]
    private GameObject templatePrefabGO;
    [SerializeField, Range(50f, 2000f), Tooltip ("Distance restricting future prefabs being instantiated to avoid overload")]
    private int renderDistance = 400;
    private List<GameObject> m_templateList = new List<GameObject>();
    private int m_templateCounter = 0;
    private Vector3 m_templateSpawnPosition;
    private Quaternion m_templateSpawnRotation;

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

    #region Obstacle Variables
    [SerializeField, Range(0.1f,1f)]
    private float obstacleDensity;
    private GameObject[] m_obstaclePrefabsGOArr;
    private float m_obstacleSpawnPosXMin;
    private float m_obstacleSpawnPosXMax;
    private float m_obstacleSpawnPosZMin;
    private float m_obstacleSpawnPosZMax;
    private Vector3 m_obstacleSpawnPosition;
    private Color32 m_obstacleColor;
    private Quaternion m_obstacleSpawnRotation;
    #endregion

    //Other
    public static ProceduralGeneration m_instance { get; private set; }

    private void Awake()
    {
        //Singleton checks
        if (m_instance == null && m_instance != this)
        {
            Destroy(m_instance);
        }
        else
        {
            m_instance = this;
        }
        //Null checks and assigning necessary references
        if (playerGO == null)
        {
            playerGO = GameObject.FindObjectOfType<PlayerController>().gameObject;
        }
        if (gameModeControllerGO == null)
        {
            gameModeControllerGO = GameObject.FindObjectOfType<GameModeController>().gameObject;
        }
        m_playerControllerRef = playerGO.GetComponent<PlayerController>();
        m_gameModeControllerRef = gameModeControllerGO.GetComponent<GameModeController>();
        //Load all available obstacles from resources and assign default obstacle color
        m_obstaclePrefabsGOArr = Resources.LoadAll<GameObject>("OBSTACLES");
        m_obstacleColor = new Color(0, 0, 0, 255);
    }
    private void Start()
    {
        SetUpInitialTemplate();
        SpawnTemplate(templatePrefabGO, m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale, m_groundColor, m_wallColor);
    }

    private void Update()
    {
        //Get current player pos
        m_playerPositionZ = m_playerControllerRef.GetPlayerPositionZ();
        //Update renderdistance relative to player position
        var render_distance_relative = m_playerPositionZ + renderDistance;
        //Get GameMode related stuff
        m_currentGameMode = m_gameModeControllerRef.GetCurrentGameMode();
        if (m_gameModeControllerRef.GameModeChanged)
        {
            UpdateTemplates();
            m_gameModeControllerRef.GameModeChanged = false;
        }
        //Debug.Log("tempSpawnPosZ: " + (m_templateSpawnPosition.z) + "playerposZ: " + m_playerPositionZ + "render dis rel: " + render_distance_relative);
        //Generate new Templates when template pos - player position reaches render distance
        if (m_templateSpawnPosition.z < render_distance_relative)
        {
            CalculateNextTemplateValues();
            //Spawn next template
            SpawnTemplate(templatePrefabGO, m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale, m_groundColor, m_wallColor);
            //Spawn obstacles
            SpawnRandomObstacles();
        }
    }

    /// <summary>
    /// Change colors of prefab on difficulty change
    /// </summary>
    private void UpdateTemplates()
    {
        //Set colors according to game mode
        //They stay the same for START and VERY_EASY
        if (m_currentGameMode == GameModes.EASY)
        {
            Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(0, 255, 250, 255);
            m_wallColor = new Color32(10, 125, 120, 255);
        }
        else if (m_currentGameMode == GameModes.MEDIUM)
        {
            Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(10, 25, 240, 255);
            m_wallColor = new Color32(10, 15, 105, 255);
        }
        else if (m_currentGameMode == GameModes.HARD)
        {
            Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(255, 155, 0, 255);
            m_wallColor = new Color32(155, 95, 10, 255);
        }
        else if(m_currentGameMode == GameModes.VERY_HARD)
        {
            Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(255, 0, 0, 255);
            m_wallColor = new Color32(115, 5, 5, 255);
        }
        else if(m_currentGameMode == GameModes.EXTREME)
        {
            Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(255, 0, 255, 255);
            m_wallColor = new Color32(150, 15, 150, 255);
        }
    }

    /// <summary>
    /// Setting up the starting template and instantiating it
    /// This will not be randomized and always look like the input prefab with adjusted spawn position
    /// </summary>
    private void SetUpInitialTemplate()
    {
        m_groundScale = new Vector3(c_originalGroundScaleX, c_originalGroundScaleY, c_originalGroundScaleZ);
        m_wallScale = new Vector3(c_originalWallScaleX, c_originalWallScaleY, c_originalWallScaleZ);
        m_spawnPositionOffsetZ =  m_groundScale.z / 2;
        m_templateSpawnPosition = playerGO.transform.position + new Vector3(0, -1.5f, m_spawnPositionOffsetZ);  //-1.5f is the height of player spawn offset
        m_templateSpawnRotation = Quaternion.identity;
        m_groundColor = new Color32(0, 255, 10, 255);
        m_wallColor = new Color32(10, 95, 2, 255);
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
        float template_spawn_posZ = m_templateList[m_templateCounter].transform.GetChild(0).GetChild(0).localScale.z ;
        m_templateSpawnPosition += new Vector3 (0,0,template_spawn_posZ + (m_groundSizeIncrease/2));
        //Increasing the increase for future spawns to counter shorter sections in higher speeds, randomize the increment
        System.Random rdm = new System.Random();
        m_groundSizeIncrease = rdm.Next(1,4);
        m_templateCounter++;
    }

    /// <summary>
    /// Spawning a template at given position, rotation and assigning the templates childs ground and wall prefab scales
    /// Calling UpdatePlayerSpeedModifier() to decrease player speed increase
    /// </summary>
    /// <param name="_templateToSpawn">Template that is spawned</param>
    /// <param name="_templateSpawnPosition">Template spawn position</param>
    /// <param name="_templateSpawnRotation">Template spawn rotation</param>
    /// <param name="_groundScaleVector">Child (GROUND) of Child (GROUND) localscale</param>
    /// <param name="_wallScaleVector">Child (WALL) of Child (WALL) localscale</param>
    /// <param name="_groundColor">Child (GROUND) of Child (GROUND) coloring</param>
    /// <param name="_wallColor">Child (WALL) of Child (Wall) coloring</param>
    private void SpawnTemplate(GameObject _templateToSpawn, Vector3 _templateSpawnPosition, Quaternion _templateSpawnRotation, Vector3 _groundScaleVector, Vector3 _wallScaleVector, Color32 _groundColor, Color32 _wallColor)
    {
        GameObject template = Instantiate(_templateToSpawn, _templateSpawnPosition, _templateSpawnRotation);
        //Update the prefabs scales
        //ground
        template.transform.GetChild(0).GetChild(0).localScale = _groundScaleVector;
        template.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = _groundColor;
        //right lower wall
        template.transform.GetChild(1).GetChild(0).localScale = _wallScaleVector;
        template.transform.GetChild(1).GetChild(0).GetComponent<Renderer>().material.color = _wallColor;
        //left lower wall
        template.transform.GetChild(1).GetChild(2).localScale = _wallScaleVector;
        template.transform.GetChild(1).GetChild(2).GetComponent<Renderer>().material.color = _wallColor;
        //Update wallScaleVector for upper walls to make them higher
        _wallScaleVector = new Vector3(_wallScaleVector.x, 7f, _wallScaleVector.z);
        //right upper wall
        template.transform.GetChild(1).GetChild(1).localScale = _wallScaleVector;
        //left upper wall
        template.transform.GetChild(1).GetChild(3).localScale = _wallScaleVector;

        //Adding spawned template to list
        m_templateList.Add(template);
        Debug.Log("Spawned Template nr."+ m_templateList.Count +" at: " + _templateSpawnPosition + " with size: " + _groundScaleVector.z);
    }

    private void SpawnRandomObstacles()
    {
        System.Random rdm = new System.Random();
        //Choose amount of obstacles depending on size of ground/template and obstacleDensity
        int min_amount_of_prefabs_to_spawn = (int) (m_groundScale.z * obstacleDensity)/2;  //restricting min amount to not be too small (like 0)
        int max_amount_of_prefabs_to_spawn = (int) (m_groundScale.z * obstacleDensity);
        int random_amount_of_prefabs_to_spawn = rdm.Next(0, max_amount_of_prefabs_to_spawn + 1);
        Debug.Log("groundscaleZ: " + m_groundScale.z + " max prefabs to spawn: " + max_amount_of_prefabs_to_spawn + " randomized: " + random_amount_of_prefabs_to_spawn);
        for (int i = 0; i < random_amount_of_prefabs_to_spawn; i++)
        {
            //Choose random obstacle prefab
            int randomPrefabNr = rdm.Next(0, m_obstaclePrefabsGOArr.Length);
            //Calculate spawn area TODO
            m_obstacleSpawnPosXMin = m_templateSpawnPosition.x - m_groundScale.x / 2;
            m_obstacleSpawnPosXMax = m_templateSpawnPosition.x + m_groundScale.x / 2;
            m_obstacleSpawnPosZMin = m_templateSpawnPosition.z - m_groundScale.z / 2;
            m_obstacleSpawnPosZMax = m_templateSpawnPosition.z + m_groundScale.z / 2;
            //Randomize spawn positions
            m_obstacleSpawnPosition = new Vector3((float)(rdm.NextDouble() * (m_obstacleSpawnPosXMax -m_obstacleSpawnPosXMin) + m_obstacleSpawnPosXMin), 0, (float)(rdm.NextDouble() * (m_obstacleSpawnPosZMax - m_obstacleSpawnPosZMin) + m_obstacleSpawnPosZMin));

            //TODO: No overlaps

            //Assign color
            m_obstaclePrefabsGOArr[randomPrefabNr].GetComponent<Renderer>().sharedMaterial.color = m_obstacleColor;
            //Assign rotation
            m_obstacleSpawnRotation = m_obstaclePrefabsGOArr[randomPrefabNr].transform.rotation;
            Instantiate(m_obstaclePrefabsGOArr[randomPrefabNr], m_obstacleSpawnPosition, m_obstacleSpawnRotation);
            Debug.Log("Spawned " + m_obstaclePrefabsGOArr[randomPrefabNr].gameObject + " at position: " + m_obstacleSpawnPosition);
        }
    }


}
