using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
 * 
 * ChangeLog
 * ----------------------------
 *  15.11.2023   FM  created, implemented spawning behaviour for Templates, 
 *                   including offsets to increase the size of the templates according to the players speed increase
 *  16.11.2023   FM  attempted fixing template spawn location
 *  17.11.2023   FM   Fixed template spawn location. the next template now always spawns at the 
 *                   end of the last one and increases it's size
 *  20.11.2023   FM  added scaling for invisible walls, fixed render distance
 *  
 *  TODO: 
 *  
 *****************************************************************************/
public class ProceduralGeneration : MonoBehaviour 
{
    [SerializeField]
    private GameObject startingTemplateGO;
    [SerializeField]
    private GameObject templateGO;
    [SerializeField, Range(50f, 2000f)]
    private float renderDistance;

    //Player Variables
    private GameObject playerGO;
    private float m_playerPositionZ;

    //Template Variables
    private List<GameObject> m_templateList = new List<GameObject>();
    private int m_templateCounter = 0;
    private GameObject m_templateToSpawnGO;
    private Vector3 m_templateSpawnPosition;
    private Quaternion m_templateSpawnRotation;
    private float m_spawnPositionOffsetZ = 0f;
    private float m_platformSizeIncrease = 5f;
    private Vector3 m_wallScale;    //stores wallScale values
    private const float c_originalWallScaleX = 1f;
    private const float c_originalWallScaleY = 1f;
    private const float c_originalWallScaleZ = 30f;
    private Vector3 m_groundScale;  //stores groundScale values
    private const float c_originalGroundScaleX = 10f;
    private const float c_originalGroundScaleY = 1f;
    private const float c_originalGroundScaleZ = 30f;

    private float m_speedModifier = 1f;

    private void Start()
    {
        playerGO = GameObject.FindAnyObjectByType<PlayerController>().gameObject;
        SetUpInitialTemplate();
        SpawnTemplate(m_templateToSpawnGO, m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale);
    }


    private void Update()
    {
        //Update Player Speed
        m_playerPositionZ = playerGO.transform.position.z;
        Debug.Log("tempSpawnPosZ: " + (m_templateSpawnPosition.z) + "playerposZ: " + m_playerPositionZ + "render dis: " + renderDistance);

        //Generate new Templates when template pos - player position reaches render distance
        if (m_templateSpawnPosition.z - m_playerPositionZ < renderDistance)
        {
            CalculateNextTemplateValues();
            //Spawn next Template
            SpawnTemplate(m_templateToSpawnGO, m_templateSpawnPosition, m_templateSpawnRotation, m_groundScale, m_wallScale);
        }
    }

    /// <summary>
    /// Setting up the starting template and instantiating it
    /// </summary>
    private void SetUpInitialTemplate()
    {
        m_templateToSpawnGO = startingTemplateGO;
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
        //Setting new template prefab
        m_templateToSpawnGO = templateGO;
        //Increasing platform size for next platform
        m_groundScale.z += m_platformSizeIncrease;
        m_wallScale.z += m_platformSizeIncrease;
        //Update templateSpawnPos according to the size increase of the next spawned platform
        var templateSpawnPosZ = m_templateList[m_templateCounter].transform.GetChild(0).GetChild(0).localScale.z ;
        m_templateSpawnPosition += new Vector3 (0,0,templateSpawnPosZ + (m_platformSizeIncrease/2));
        //Increasing the increase for future spawns to counter shorter sections in higher speeds
        m_platformSizeIncrease += 2;
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
