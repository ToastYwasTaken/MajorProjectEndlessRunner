using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
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
* ----------------------------
* Script Description:
* Controlls procedural generation by randomly generating certain constraints and limiting the bounds of the procedural generator: 
*           - Templates (consisting of Ground + Wall GOs):
*                   - spawnposition.z
*                   - scale
*                   - color
*           - Obstacles: 
*                  - spawnposition 
*                  - density
*                  - GO Type
*                  - color
* The length of the templates increases over time to adjust to the player's speed increase
* 
* ----------------------------
* ChangeLog:
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
*  27.11.2023   FM  implemented obstacle spawning, tweaked constraints and restricted spawn overlaps by raycasting
*  03.12.2023   FM  exported obstacle spawning to ObjectSpawner.cs; adjusted functionality in here; tweaked obstacle spawn rate
*  
*  TODO:
*       - tweak constraints for Obstacle Spawning - done
*  Buglist:
*       - resolve accessing variables from other GO not working - resolved
*  
*****************************************************************************/
public class ProceduralGeneration : MonoBehaviour 
{
    public static ProceduralGeneration Instance { get; private set; }

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
    [SerializeField, Range(50f, 2000f), Tooltip("Distance restricting active GO in scene to avoid overload")]
    private int renderDistance = 400;
    private float m_playerPositionZ;
    #endregion

    #region Template Variables
    private int m_templateCounter = 0;
    private Vector3 m_templateSpawnPosition;
    private Quaternion m_templateSpawnRotation;

    //Wall Variables
    private Vector3 m_wallScale;
    //Default values for walls
    private const float c_originalWallScaleX = 1f;
    private const float c_originalWallScaleY = 1f;
    private const float c_originalWallScaleZ = 30f;
    private Color32 m_wallColor;
    //Ground Variables
    private Vector3 m_groundScale;  
    //Default values for ground
    private const float c_originalGroundScaleX = 10f;
    private const float c_originalGroundScaleY = 1f;
    private const float c_originalGroundScaleZ = 30f;
    private float m_distanceToGround = 1.5f;
    private float m_groundSizeIncrease = 5f;
    private Color32 m_groundColor;
    #endregion

    #region Obstacle Variables
    [SerializeField, Range(0.1f,1f), Tooltip("Manually controlled density of random obstacles to spawn")]
    private float obstacleDensity;
    [SerializeField, Tooltip("Size of box to check for colliders with raycast")]
    private Vector3 overlapBoxScale;
    [SerializeField, Tooltip("Layer of colliders to check for inside the overlap box")]
    private LayerMask rayCastLayer;
    private float m_obstacleSpawnPosXMin;
    private float m_obstacleSpawnPosXMax;
    private float m_obstacleSpawnPosZMin;
    private float m_obstacleSpawnPosZMax;
    private Color32 m_obstacleColor;
    #endregion

    #region Other
    [SerializeField, Tooltip("Object spawner GO")]
    private GameObject objectSpawnerGO;
    private ObjectSpawner m_objectSpawnerRef;
    private bool m_inPlayMode = false;
    #endregion
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
        //Null checks and assigning necessary references
        if (gameModeControllerGO != null) { m_gameModeControllerRef = gameModeControllerGO.GetComponent<GameModeController>(); }
        if(playerGO != null) { m_playerControllerRef = playerGO.GetComponent<PlayerController>(); }
        if(objectSpawnerGO != null) { m_objectSpawnerRef = objectSpawnerGO.GetComponent<ObjectSpawner>(); }
        m_obstacleColor = new Color(0, 0, 0, 255);
    }
    private void Start()
    {
        m_inPlayMode = true;
        CalculateStartingTemplate();
        m_objectSpawnerRef.SpawnTemplate(m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale, m_groundColor, m_wallColor, true);
    }

    private void Update()
    {
        //Get GameMode related stuff
        m_currentGameMode = m_gameModeControllerRef.GetCurrentGameMode();
        if (m_gameModeControllerRef.GetGameModeChanged())
        {
            UpdateTemplateColor();
            m_gameModeControllerRef.SetGameModeChanged(false);
        }
        //Debug.Log("tempSpawnPosZ: " + (m_templateSpawnPosition.z) + " playerPosZ: " + m_playerPositionZ);
        //Update playerPos and renderdistance relative to player position
        m_playerPositionZ = m_playerControllerRef.GetPlayerPosition().z;
        var render_distance_relative = m_playerPositionZ + renderDistance;
        if(m_templateSpawnPosition.z < render_distance_relative)
        {
            CalculateNextTemplateValues();
            //Spawn next template
            m_objectSpawnerRef.SpawnTemplate(m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale, m_groundColor, m_wallColor, false);
            //Spawn obstacles
            CalculateRandomObstacles();
            m_objectSpawnerRef.SaveTemplateAndObstaclesToDic();
            m_objectSpawnerRef.DeleteUnusedObjects(m_playerPositionZ);
        }
    }



    /// <summary>
    /// Change colors of prefab on difficulty change
    /// </summary>
    private void UpdateTemplateColor()
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
    private void CalculateStartingTemplate()
    {
        m_groundScale = new Vector3(c_originalGroundScaleX, c_originalGroundScaleY, c_originalGroundScaleZ);
        m_wallScale = new Vector3(c_originalWallScaleX, c_originalWallScaleY, c_originalWallScaleZ);
        float spawn_position_offset_z =  m_groundScale.z / 2;
        m_templateSpawnPosition = playerGO.transform.position + new Vector3(0, -m_distanceToGround, spawn_position_offset_z);  
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
        float template_spawn_posZ = m_objectSpawnerRef.GetAllPCGObjects()[m_templateCounter].GetTemplateGO().transform.GetChild(0).GetChild(0).localScale.z ;
        m_templateSpawnPosition += new Vector3 (0,0,template_spawn_posZ + (m_groundSizeIncrease/2));
        //Increasing the increase for future spawns to counter shorter sections in higher speeds, randomize the increment
        System.Random rdm = new System.Random();
        m_groundSizeIncrease = rdm.Next(1,4);
        m_templateCounter++;
    }

    /// <summary>
    /// This randomizes the spawning process for obstacles, taking into account the bounds of walls,
    /// ignoring the starting template, and spawning more obstacle with higher template length and player speed
    /// </summary>
    private void CalculateRandomObstacles()
    {
        System.Random rdm = new System.Random();
        //Choose amount of obstacles depending on size of ground/template and obstacleDensity
        int min_amount_of_prefabs_to_spawn = (int) (m_groundScale.z * obstacleDensity *((int)m_currentGameMode+1) * 0.1f)/2;  //restricting min amount to not be too small (like 0)
        int max_amount_of_prefabs_to_spawn = (int) (m_groundScale.z * obstacleDensity *((int)m_currentGameMode+1) * 0.1f);
        int random_amount_of_prefabs_to_spawn = rdm.Next(min_amount_of_prefabs_to_spawn, max_amount_of_prefabs_to_spawn);
        //Debug.Log("groundscaleZ: " + m_groundScale.z +" min prefabs to spawn: " + min_amount_of_prefabs_to_spawn + " max prefabs to spawn: " + max_amount_of_prefabs_to_spawn + " randomized: " + random_amount_of_prefabs_to_spawn);
        int obstaclesSpawnedCounter = 0;
        for (int i = 0; i < random_amount_of_prefabs_to_spawn; i++)
        {
            //Choose random obstacle prefab
            int random_prefab_nr = rdm.Next(0, m_objectSpawnerRef.GetObstaclePrefabsArr().Length);
            //offset needed to not spawn obstacles overlapping with the wall or obstacles from another template
            float obstacle_spawn_offset = 1f;
            //Calculate spawn area
            m_obstacleSpawnPosXMin = m_templateSpawnPosition.x - m_groundScale.x / 2 + obstacle_spawn_offset;   
            m_obstacleSpawnPosXMax = m_templateSpawnPosition.x + m_groundScale.x / 2 - obstacle_spawn_offset;
            m_obstacleSpawnPosZMin = m_templateSpawnPosition.z - m_groundScale.z / 2 + obstacle_spawn_offset;
            m_obstacleSpawnPosZMax = m_templateSpawnPosition.z + m_groundScale.z / 2 - obstacle_spawn_offset;
            //Randomize spawn positions
            var obstacle_spawn_position = new Vector3((float)(rdm.NextDouble() * (m_obstacleSpawnPosXMax -m_obstacleSpawnPosXMin) + m_obstacleSpawnPosXMin), m_distanceToGround, (float)(rdm.NextDouble() * (m_obstacleSpawnPosZMax - m_obstacleSpawnPosZMin) + m_obstacleSpawnPosZMin));

            //Cast Raycast from current desired spawn position to check for overlaps with other obstacles
            RaycastHit hit;
            if(Physics.Raycast(obstacle_spawn_position, Vector3.down, out hit, m_groundScale.z))
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Collider[] hit_colliders = new Collider[1];
                int colliders_found = Physics.OverlapBoxNonAlloc(hit.point, overlapBoxScale, hit_colliders, rotation, rayCastLayer);
                
                //No overlaps found -> instantiate the obstacle
                if (colliders_found == 0)
                {
                    //Spawn Obstacle
                    m_objectSpawnerRef.SpawnObstacle(obstacle_spawn_position, Quaternion.identity, m_obstacleColor, random_prefab_nr);
                    obstaclesSpawnedCounter++;
                }
            }
        }
            //Debug.Log("Actually spawned: " + obstaclesSpawnedCounter + " obstacles");
    }
}
