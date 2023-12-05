using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
* Responsible for spawning and despawning the objects requested from ProceduralGeneration.cs
* ----------------------------
* ChangeLog:
*  03.12.2023   FM  created; imported functionality from ProceduralGeneration.cs to split up spawning from procedural generation
*  04.12.2023   FM  added changing of instantiated object names; added structuring of hierarchy by setting the templates OBSTACLES child as parent;
*                   fixed saving of templates and obstacles in dictionary
*  
*  TODO:
*       - Change names from prefabs when spawning - done
*       - Properly Debug the dictionary
*  Buglist:
*       - 
*  
*****************************************************************************/
public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }
    [SerializeField, Tooltip("Prefab of basic template to be instantiated")]
    private GameObject templatePrefabGO;
    private GameObject[] m_obstaclePrefabsGOArr;
    private Dictionary<int, TemplateWithObstacles> m_pcgObjectsDic = new Dictionary<int, TemplateWithObstacles>();
    private int m_dictionaryID = 0;
    private GameObject m_currentTemplateGO;
    private List<GameObject> m_currentObstacleList = new List<GameObject>();
    private int m_templateCounter = 0;
    private int m_obstacleCounter = 0;
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
        //Clear obstaclelist
        m_currentObstacleList.Clear();
        m_currentTemplateGO = Instantiate(templatePrefabGO, _spawnPosition, _spawnRotation);
        //Update GO values
        //name
        m_currentTemplateGO.name = "Template(DEFAULT)[" + m_templateCounter +"]";
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
            //adding starting template without obstacles; if it's not, it will get added along with it's obstacles in SpawnObstacle()
            m_pcgObjectsDic.Add(m_dictionaryID, new TemplateWithObstacles(m_currentTemplateGO, new List<GameObject>()));
            m_dictionaryID++;
        }
        m_templateCounter++;
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
        //Update GO values
        current_obstacle.name = "Obstacle("+ m_obstaclePrefabsGOArr[_selectedPrefabID].gameObject.name + ")[" + m_obstacleCounter + "]";
        //Assign color
        current_obstacle.GetComponent<Renderer>().material.color = _color;
        //Add obstacles to the according template to not cludder the hierarchy
        current_obstacle.transform.parent = m_currentTemplateGO.transform.GetChild(2);
        //add to obstacleList
        m_currentObstacleList.Add(current_obstacle);
        m_obstacleCounter++;
    }

    /// <summary>
    /// Saves the instantiated template with it's obstacles to a dictionary
    /// </summary>
    public void SaveTemplateAndObstaclesToDic()
    {
        //Create new object storing templateGO and all obstacles that were spawned on it
        TemplateWithObstacles templateWithObstacles = new TemplateWithObstacles(m_currentTemplateGO, m_currentObstacleList);
        //Adding the object to the dictionary
        m_pcgObjectsDic.Add(m_dictionaryID, templateWithObstacles);
        //string debugstring = "";
        //for (int i = 0; i < procedurallyGeneratedObjectsDic.Count; i++)
        //{
        //    string debug_template = procedurallyGeneratedObjectsDic[i].GetTemplateGO().gameObject.name;
        //    debugstring += "Key[" + i + "] is '" + debug_template + "' and contains a total of "+ procedurallyGeneratedObjectsDic[i].GetObstacleList().Count + " values: ";

        //    for (int j = 0; j < procedurallyGeneratedObjectsDic[i].GetObstacleList().Count; j++)
        //    {
        //        string debug_obstacle = procedurallyGeneratedObjectsDic[i].GetObstacleList()[j].gameObject.name;
        //        debugstring += "Value["+ j +"]Obstacle named '" + debug_obstacle + "'";
        //    }
        //}
        //Debug.Log(debugstring);
        m_dictionaryID++;
        //reset obstacle counter
        m_obstacleCounter = 0;
    }

    /// <summary>
    /// Removes all unused objects from the dictionary and deletes them
    /// </summary>
    /// <param name="_playerPosZ"></param>
    public void DeleteUnusedObjects(float _playerPosZ)
    {

        var matches = m_pcgObjectsDic.Where(item => item.Value.GetTemplateGO().transform.position.z < _playerPosZ);
        
        matches.ToDictionary(item => item.Key, item => item.Value);
    }

    public Dictionary<int, TemplateWithObstacles> GetAllPCGObjects() { return m_pcgObjectsDic;}
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