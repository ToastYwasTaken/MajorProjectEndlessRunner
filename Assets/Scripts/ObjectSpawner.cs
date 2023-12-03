using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VFX;

/// <summary>
/// Holds GameObject corresponding to 1 game tile, consisting of
/// a template containing walls and ground, and the obstacles generated on it
/// </summary>
public class TemplateWithObstacles
{
    private GameObject templateGO;
    private List<GameObject> obstacleList = new List<GameObject>();

    public TemplateWithObstacles(GameObject _templateGO, List<GameObject> _obstacleList)
    {
        templateGO = _templateGO;
        obstacleList = _obstacleList;
    }
}

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner m_instance { get; private set; }

    private Dictionary<int, TemplateWithObstacles> procedurallyGeneratedObjectsDic = new Dictionary<int, TemplateWithObstacles>();
    private int m_dictionaryID = 0;
    private GameObject m_currentGO;

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
    }

    public void SpawnObstacles(GameObject _prefabToSpawn, Vector3 _spawnPosition, Quaternion _spawnRotation, Color32 color)
    {
        List<GameObject> obstacle_list = new List<GameObject>();
        
  
        //Create new object storing template go and all obstacles on it
        TemplateWithObstacles templateWithObstacles = new TemplateWithObstacles(m_currentGO, obstacle_list);
        //Adding the object to the dictionary
        procedurallyGeneratedObjectsDic.Add(m_dictionaryID, templateWithObstacles);
        m_dictionaryID++;
        
    }
    /// <summary>
    /// Spawning a template at given position, rotation and assigning the templates childs ground and wall prefab scales
    /// </summary>
    /// <param name="_prefabToSpawn">Template that is spawned</param>
    /// <param name="_spawnPosition">Template spawn position</param>
    /// <param name="_spawnRotation">Template spawn rotation</param>
    /// <param name="_groundScale">Child (GROUND) of Child (GROUND) localscale</param>
    /// <param name="_wallScale">Child (WALL) of Child (WALL) localscale</param>
    /// <param name="_groundColor">Child (GROUND) of Child (GROUND) coloring</param>
    /// <param name="_wallColor">Child (WALL) of Child (Wall) coloring</param>
    public void SpawnTemplate(GameObject _prefabToSpawn, Vector3 _spawnPosition, Quaternion _spawnRotation, Vector3 _groundScale, Vector3 _wallScale, Color32 _groundColor, Color32 _wallColor)
    {
        m_currentGO = Instantiate(_prefabToSpawn, _spawnPosition, _spawnRotation);
        //Update the prefabs scales
        //ground
        m_currentGO.transform.GetChild(0).GetChild(0).localScale = _groundScale;
        m_currentGO.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = _groundColor;
        //right lower wall
        m_currentGO.transform.GetChild(1).GetChild(0).localScale = _wallScale;
        m_currentGO.transform.GetChild(1).GetChild(0).GetComponent<Renderer>().material.color = _wallColor;
        //left lower wall
        m_currentGO.transform.GetChild(1).GetChild(2).localScale = _wallScale;
        m_currentGO.transform.GetChild(1).GetChild(2).GetComponent<Renderer>().material.color = _wallColor;
        //Update wallScaleVector for upper walls to make them higher
        _wallScale = new Vector3(_wallScale.x, 7f, _wallScale.z);
        //right upper wall
        m_currentGO.transform.GetChild(1).GetChild(1).localScale = _wallScale;
        //left upper wall
        m_currentGO.transform.GetChild(1).GetChild(3).localScale = _wallScale;
    }
}
