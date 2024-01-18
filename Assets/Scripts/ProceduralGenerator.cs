using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using UnityEngine;
/******************************************************************************
* Project: MajorProjectEndlessRunner
* File: ProceduralGenerator.cs
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
*                   - amount
*                   - spawnposition 
*                   - density
*                   - GO Type
*                   - color
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
*  09.12.2023   FM  modified PCG algorithm; added defaultObstacleSpawnValue
*  10.12.2023   FM  improved calculation of obstacle spawn pos min and max value; still have to fix the randomization
*  12.12.2023   FM  improved spawn pos calculation by adding another OverlapSphere to avoid nearby obstacle spawning, removed alternative calculation for obstacle spawn pos randomization
*  14.12.2023   FM  added MapObstacleSpawnRange() to generate better amounts of spawned obstacles, added rotated Quads as obstacles
*  26.12.2023   FM  exported most Helper functions to MapperAndRdmHelper.cs
*  
*  TODO:
*       - tweak constraints for Obstacle Spawning - done
*       - fix randomization bias - done
*       - export some functionality in CalculateRandomObstacles() to clean up code - done
*       - tweak obstacle counter - done
*       - add spawn rarity for obstacles - OPTIONAL
*  Buglist:
*       - resolve accessing variables from other GO not working - resolved
*       - obstacle spawn pos randomization spawning to cluttered around center due to faulty calculation - resolved
*  
*****************************************************************************/
public class ProceduralGenerator : MonoBehaviour 
{
    public static ProceduralGenerator Instance { get; private set; }

    #region GameController Variables
    [SerializeField, Tooltip("GameModeController GameObject in game scene, gets assigned in Awake() if unassigned")]
    private GameObject gameModeControllerGO;
    private GameModeController m_gameModeControllerRef;
    private EGameModes m_currentGameMode;
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
    private float m_groundSizeIncrease = 0f;
    private Color32 m_groundColor;
    #endregion

    #region Obstacle Variables
    public float ObstacleDensity { get { return m_obstacleDensity;} set{ m_obstacleDensity = value; } }
    private float m_obstacleDensity = 1f;
    [SerializeField, Tooltip("Size of box to check for colliders, used to avoid spawn overlaps")]
    private Vector3 overlapBoxScale;
    [SerializeField, Tooltip("Size of box to check for colliders, used to reduce chance of 'near' spawning")]
    private Vector3 rerollBoxScale;
    [SerializeField, Tooltip("Layer of colliders to check for inside the overlap box")]
    private LayerMask rayCastLayer;
    private int c_defaultObstacleSpawnValue = 2;
    private Color32 m_obstacleColor;
    private int c_minObstacleAmount = 1;
    private int c_minRequiredRangeToMap = 5;
    #endregion

    #region Other
    [SerializeField, Tooltip("Object spawner GO")]
    private GameObject objectSpawnerGO;
    private ObjectSpawner m_objectSpawnerRef;
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
        CalculateStartingTemplate();
        m_objectSpawnerRef.SpawnTemplate(m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale, m_groundColor, m_wallColor, true);
        //Load DDA values
        m_obstacleDensity = DynamicDifficultyAdjuster.ObstacleDensity;
        //Debug.Log("Obstacle Density: " + m_obstacleDensity);
    }

    private void Update()
    {
        //Get GameMode related stuff
        m_currentGameMode = m_gameModeControllerRef.currentGameMode;
        if (m_gameModeControllerRef.gameModeChanged)
        {
            UpdateTemplateColor();
            m_gameModeControllerRef.gameModeChanged = false;
        }
        //Debug.Log("tempSpawnPosZ: " + (m_templateSpawnPosition.z) + " playerPosZ: " + m_playerPositionZ);
        //Update playerPos and renderdistance relative to player position
        m_playerPositionZ = m_playerControllerRef.PlayerPosition.z;
        var render_distance_relative = m_playerPositionZ + renderDistance;
        if(m_templateSpawnPosition.z < render_distance_relative)
        {
            CalculateNextTemplateValues();
            //Spawn next template
            m_objectSpawnerRef.SpawnTemplate(m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale, m_groundColor, m_wallColor, false);
            //Debug.Log("TempSpawnPos: " + m_templateSpawnPosition + " - groundsizeIncrease: " + m_groundSizeIncrease);
            //Spawn obstacles
            CalculateRandomObstacles();
            m_objectSpawnerRef.SaveTemplateAndObstaclesToList();
        }
            //delete unused objects behind player; update list, update templateCounter
            int deleted_templates_count = m_objectSpawnerRef.DeleteUnusedObjects(m_playerPositionZ);
            //Debug.Log("deleted templates count: " + deleted_templates_count);
            m_templateCounter -= deleted_templates_count;
    }

    /// <summary>
    /// Change colors of prefab on difficulty change
    /// </summary>
    private void UpdateTemplateColor()
    {
        //Set colors according to game mode
        //They stay the same for START and VERY_EASY
        if (m_currentGameMode == EGameModes.EASY)
        {
            //Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(0, 255, 250, 255);
            m_wallColor = new Color32(10, 125, 120, 255);
        }
        else if (m_currentGameMode == EGameModes.MEDIUM)
        {
            //Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(10, 25, 240, 255);
            m_wallColor = new Color32(10, 15, 105, 255);
        }
        else if (m_currentGameMode == EGameModes.HARD)
        {
            //Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(255, 155, 0, 255);
            m_wallColor = new Color32(155, 95, 10, 255);
        }
        else if(m_currentGameMode == EGameModes.VERY_HARD)
        {
            //Debug.Log("Swapped color for GM: " + m_currentGameMode);
            m_groundColor = new Color32(255, 0, 0, 255);
            m_wallColor = new Color32(115, 5, 5, 255);
        }
        else if(m_currentGameMode == EGameModes.EXTREME)
        {
            //Debug.Log("Swapped color for GM: " + m_currentGameMode);
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
        //Increasing the size increase for future spawns to counter shorter sections in higher speeds, randomize the increment
        System.Random rdm = new System.Random();
        //Calculate ground size increase to increase proportional to player's speed
        int player_speed_rounded = (int)m_playerControllerRef.SpeedVertical;
        //Debug.Log((int)m_playerControllerRef.speedVertical);
        int min_ground_size_increase = player_speed_rounded / 2;
        m_groundSizeIncrease = rdm.Next(min_ground_size_increase,player_speed_rounded);
        m_templateCounter++;
    }

    /// <summary>
    /// This randomizes the spawning process for obstacles, taking into account the bounds of walls,
    /// ignoring the starting template, and spawning more obstacle with higher template length and player speed
    /// </summary>
    private void CalculateRandomObstacles()
    {
        System.Random rdm = new System.Random();
        //Calculate max spawn amount
        int max_amount_of_prefabs_to_spawn = MapperAndRdmHelper.CalculateObstacleMaxSpawnAmount(c_defaultObstacleSpawnValue, m_groundScale.z, c_originalGroundScaleZ, m_obstacleDensity, m_currentGameMode);
        //Remap the "big" range to a smaller range to narrow randomization range
        (int,int) amount_of_prefabs_to_spawn_range_mapped = MapperAndRdmHelper.ReduceRangeByFactor(c_minObstacleAmount, max_amount_of_prefabs_to_spawn, 0.4f, c_minRequiredRangeToMap);
        //Randomize range
        int random_amount_of_prefabs_to_spawn = rdm.Next(amount_of_prefabs_to_spawn_range_mapped.Item1, amount_of_prefabs_to_spawn_range_mapped.Item2);
        //Debug.Log("groundscaleZ: " + m_groundScale.z + "gameModeModifier: "+ (1 + (int)m_currentGameMode) * 0.1f + " min prefabs to spawn: " + min_amount_of_prefabs_to_spawn + " max prefabs to spawn: " + max_amount_of_prefabs_to_spawn + " randomized: " + random_amount_of_prefabs_to_spawn);
        int obstacles_spawned_counter = 0;
        for (int i = 0; i < random_amount_of_prefabs_to_spawn; i++)
        {
            //Choose random obstacle prefab
            int random_prefab_nr = rdm.Next(0, m_objectSpawnerRef.GetObstaclePrefabsArr().Length);
            //offset needed to not spawn obstacles overlapping with the wall or obstacles from another template
            float obstacle_spawn_offset = 1.5f;
            //Calculate spawn area
            float obstacle_spawn_pos_x_min = m_templateSpawnPosition.x - m_groundScale.x / 2 + obstacle_spawn_offset;   
            float obstacle_spawn_pos_x_max = m_templateSpawnPosition.x + m_groundScale.x / 2 - obstacle_spawn_offset;
            //no offset needed here therefore obstacles can also spawn between templates
            float obstacle_spawn_pos_z_min = m_templateSpawnPosition.z - m_groundScale.z / 2;
            float obstacle_spawn_pos_z_max = m_templateSpawnPosition.z + m_groundScale.z / 2;
            Vector3 obstacle_spawn_pos;
            bool spawned_successfully = false;
            int break_value = 1000;
            int attempts = 0;

            while (!spawned_successfully && attempts < break_value)
            {
                //Randomize spawn positions
                float obstacle_spawn_pos_x = MapperAndRdmHelper.RandomizeFloat(obstacle_spawn_pos_x_min, obstacle_spawn_pos_x_max);
                float obstacle_spawn_pos_z = MapperAndRdmHelper.RandomizeFloat(obstacle_spawn_pos_z_min, obstacle_spawn_pos_z_max);
                obstacle_spawn_pos = new Vector3(obstacle_spawn_pos_x, m_distanceToGround, obstacle_spawn_pos_z);
                //Reroll check
                Vector3 box_position = obstacle_spawn_pos;
                Collider[] hit_colliders = new Collider[1];
                Quaternion randomized_spawn_rotation = MapperAndRdmHelper.RandomizeQuaternionY();
                int colliders_found = Physics.OverlapBoxNonAlloc(box_position, rerollBoxScale, hit_colliders, Quaternion.identity, rayCastLayer);
                //randomize if reroll check is successful // chance 4 : 1 to reroll
                if (colliders_found != 0 && rdm.Next(0, 5) != 0)
                {
                    attempts++;
                    continue;
                    //Debug.Log("SpawnPos was rerolled due to successful collider and random value check");
                }
                else
                {
                    //Overlap check
                    hit_colliders = new Collider[1];
                    colliders_found = Physics.OverlapBoxNonAlloc(box_position, overlapBoxScale, hit_colliders, Quaternion.identity, rayCastLayer);
                    //No overlaps found -> instantiate the obstacle
                    if (colliders_found == 0)
                    {
                        //Spawn Obstacle
                        m_objectSpawnerRef.SpawnObstacle(obstacle_spawn_pos, randomized_spawn_rotation, m_obstacleColor, random_prefab_nr);
                        obstacles_spawned_counter++;
                        spawned_successfully = true;  
                        //Debug.Log(" Z Min: " + obstacle_spawn_pos_z_min + " Z max: " + obstacle_spawn_pos_z_max + " actual spawn pos Z: " + obstacle_spawn_pos_z + " SPAWNED");
                    } //else Debug.Log(" Z Min: " + obstacle_spawn_pos_z_min + " Z max: " + obstacle_spawn_pos_z_max + " actual spawn pos Z: " + obstacle_spawn_pos_z + " IGNORED");
                    attempts++;
                }
            }
        }
        //Debug.Log("Actually spawned: " + obstacles_spawned_counter + " obstacles");
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireCube(box_position, overlapBoxScale);
    }
}
