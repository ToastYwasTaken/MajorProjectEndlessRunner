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
 *  16.11.2023   FM  added proper spawn location for templates
 *  
 *  TODO: Fix Render distance
 *  
 *****************************************************************************/
public class ProceduralGeneration : MonoBehaviour 
{
    [SerializeField]
    private GameObject playerRef;
    [SerializeField]
    private GameObject startingTemplateRef;
    [SerializeField]
    private GameObject templateRef;

    private List<GameObject> m_templateList = new List<GameObject>();
    private float m_playerPositionZ;
    private const float c_renderDistance = 40f;

    //Template Variables
    private GameObject m_templateToSpawn;
    private Vector3 m_templateSpawnPosition;
    private Quaternion m_templateSpawnRotation;
    private float m_groundScaleZ;
    private bool m_templateSpawned = false;
    private int m_templateCounter = 0;
    private float m_templateLengthToSpeedRatio;
    private float m_spawnPositionOffsetZ = 0f;
    private const float c_templateScaleX = 10f;
    private const float c_templateScaleY = 1f;


    private float m_speedModifier = 1f;
    private float m_platformSizeIncrease = 5f;

    private void Start()
    {
        SetUpInitialTemplate();
        SpawnTemplate(m_templateToSpawn, m_templateSpawnPosition, m_templateSpawnRotation);
    }


    private void Update()
    {
        //Update Player Speed
        m_playerPositionZ = playerRef.transform.position.z;
        Debug.Log("tempSpawnPos: " + m_templateSpawnPosition.z + " playerPos: " + m_playerPositionZ);

        //Generate new Templates while renderdistance is not too far away
        if (m_templateSpawnPosition.z - m_playerPositionZ < c_renderDistance && !m_templateSpawned)
        {
            CalculateNextTemplateValues();
            //Spawn next Template
            SpawnTemplate(m_templateToSpawn, m_templateSpawnPosition, m_templateSpawnRotation);
            //Updating templates
            m_templateSpawned = true;
            //UpdatePlayerSpeedModifier();
        }
        if (m_playerPositionZ == m_templateSpawnPosition.z)
        {
            m_templateSpawned = false;
        }
    }

    /// <summary>
    /// Setting up the starting template and instantiating it
    /// </summary>
    private void SetUpInitialTemplate()
    {
        m_templateToSpawn = startingTemplateRef;
        m_groundScaleZ = startingTemplateRef.transform.GetChild(0).GetChild(0).localScale.z;
        m_spawnPositionOffsetZ = m_groundScaleZ / 2;
        m_templateSpawnPosition = playerRef.transform.position + new Vector3(0, -1.5f, m_spawnPositionOffsetZ);  //-1.5f is the height of player spawn offset
        m_templateSpawnRotation = Quaternion.identity;
    }


    /// <summary>
    /// Updating m_groundScaleZ, m_templateSpawnPosition and calculating m_templateLengthToSpeedRatio which increases 
    /// with increasing player speed, also resulting in longer templates being instantiated
    /// </summary>
    private void CalculateNextTemplateValues()
    {
        m_templateLengthToSpeedRatio = playerRef.GetComponent<PlayerController>().GetVerticalSpeed() / m_groundScaleZ;
        //Update groundScaleZ
        m_templateSpawnPosition = new Vector3 (0,0,m_groundScaleZ) + m_templateSpawnPosition;
        //Increasing platform size for next platform
        m_groundScaleZ += m_platformSizeIncrease;
        //Increasing the increase for future spawns to counter shorter sections in higher speeds
        m_platformSizeIncrease += 2;
    }

    private void SpawnTemplate(GameObject _templateToSpawn, Vector3 _templateSpawnPosition, Quaternion _templateSpawnRotation)
    {
        var template = Instantiate(_templateToSpawn, _templateSpawnPosition, _templateSpawnRotation);
        m_templateList.Add(template);
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
