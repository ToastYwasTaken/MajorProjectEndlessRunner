using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

/******************************************************************************
* Project: MajorProjectEndlessRunner
* File: ObjectSpawner.cs
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
* 
* ----------------------------
* ChangeLog:
*  03.12.2023   FM  created; imported functionality from ProceduralGeneration.cs to split up spawning from procedural generation
*  
*  TODO:
*       - Change names from prefabs when spawning
*       - Properly Debug the dictionary
*  Buglist:
*       - 
*  
*****************************************************************************/
public class ObjectSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("Prefab of basic template to be instantiated")]
    private GameObject templatePrefabGO;
    public static ObjectSpawner s_instance { get; private set; }
    private GameObject[] m_obstaclePrefabsGOArr;
    private Dictionary<int, TemplateWithObstacles> procedurallyGeneratedObjectsDic = new Dictionary<int, TemplateWithObstacles>();
    private int m_dictionaryID = 0;
    private GameObject m_currentTemplateGO;
    private List<GameObject> m_currentObstacleList = new List<GameObject>();
    private void Awake()
    {
        //Singleton checks
        if (s_instance == null && s_instance != this)
        {
            Destroy(s_instance);
        }
        else
        {
            s_instance = this;
        }
        //Load all available obstacles from resources and assign default obstacle color
        m_obstaclePrefabsGOArr = Resources.LoadAll<GameObject>("OBSTACLES");
    }

    /// <summary>
    /// Spawning a template at given position, rotation and assigning the templates childs ground and wall prefab scales
    /// </summary>
    /// <param name="_spawnPosition">Template spawn position</param>
    /// <param name="_spawnRotation">Template spawn rotation</param>
    /// <param name="_groundScale">Child (GROUND) of Child (GROUND) localscale</param>
    /// <param name="_wallScale">Child (WALL) of Child (WALL) localscale</param>
    /// <param name="_groundColor">Child (GROUND) of Child (GROUND) coloring</param>
    /// <param name="_wallColor">Child (WALL) of Child (Wall) coloring</param>
    /// <param name="_isStartingTemplate">Starting template only spawns a template with no obstacles</param>
    public void SpawnTemplate(Vector3 _spawnPosition, Quaternion _spawnRotation, Vector3 _groundScale, Vector3 _wallScale, Color32 _groundColor, Color32 _wallColor, bool _isStartingTemplate)
    {
        m_currentTemplateGO = Instantiate(templatePrefabGO, _spawnPosition, _spawnRotation);
        //Update the prefabs scales
        //ground
        m_currentTemplateGO.transform.GetChild(0).GetChild(0).localScale = _groundScale;
        m_currentTemplateGO.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = _groundColor;
        //right lower wall
        m_currentTemplateGO.transform.GetChild(1).GetChild(0).localScale = _wallScale;
        m_currentTemplateGO.transform.GetChild(1).GetChild(0).GetComponent<Renderer>().material.color = _wallColor;
        //left lower wall
        m_currentTemplateGO.transform.GetChild(1).GetChild(2).localScale = _wallScale;
        m_currentTemplateGO.transform.GetChild(1).GetChild(2).GetComponent<Renderer>().material.color = _wallColor;
        //Update wallScaleVector for upper walls to make them higher
        _wallScale = new Vector3(_wallScale.x, 7f, _wallScale.z);
        //right upper wall
        m_currentTemplateGO.transform.GetChild(1).GetChild(1).localScale = _wallScale;
        //left upper wall
        m_currentTemplateGO.transform.GetChild(1).GetChild(3).localScale = _wallScale;
        if (_isStartingTemplate)
        {
            //adding starting template without obstacles
            procedurallyGeneratedObjectsDic.Add(m_dictionaryID, new TemplateWithObstacles(m_currentTemplateGO, new List<GameObject>()));
            m_dictionaryID++;
        }
    }

    /// <summary>
    /// Spawns an obstacle on top of the Template prefab
    /// </summary>
    /// <param name="_spawnPosition"></param>
    /// <param name="_spawnRotation"></param>
    /// <param name="_color"></param>
    /// <param name="_selectedPrefabID"></param>
    public void SpawnObstacle(Vector3 _spawnPosition, Quaternion _spawnRotation, Color32 _color, int _selectedPrefabID)
    {
        GameObject current_obstacle = Instantiate(m_obstaclePrefabsGOArr[_selectedPrefabID], _spawnPosition, _spawnRotation);
        //Assign color
        current_obstacle.GetComponent<Renderer>().material.color = _color;
        //add to obstacleList
        //TODO: Only adds 1 obstacle per time now
        m_currentObstacleList.Add(current_obstacle);
        //Create new object storing templateGO and all obstacles that were spawned on it
        TemplateWithObstacles templateWithObstacles = new TemplateWithObstacles(m_currentTemplateGO, m_currentObstacleList);
        //Adding the object to the dictionary
        procedurallyGeneratedObjectsDic.Add(m_dictionaryID, templateWithObstacles);
        m_dictionaryID++;
        for (int i = 0; i < procedurallyGeneratedObjectsDic.Count; i++)
        {
            Debug.Log(procedurallyGeneratedObjectsDic[i]);
        }
    }
    public Dictionary<int, TemplateWithObstacles> GetAllPCGObjects() { return procedurallyGeneratedObjectsDic;}
    public GameObject[] GetObstaclePrefabsArr() { return m_obstaclePrefabsGOArr; }
}

/// <summary>
/// Holds GameObject corresponding to 1 game tile, consisting of
/// a template containing walls and ground, and the obstacles generated on it
/// </summary>
public class TemplateWithObstacles
{
    private GameObject templateGO;
    private List<GameObject> obstacleList = new List<GameObject>(); //Have to use List since the length is not fixed

    public TemplateWithObstacles(GameObject _templateGO, List<GameObject> _obstacleList)
    {
        templateGO = _templateGO;
        obstacleList = _obstacleList;
    }
    public GameObject GetTemplateGO() { return templateGO; }
    public List<GameObject> GetObstacleList() { return obstacleList; }
}