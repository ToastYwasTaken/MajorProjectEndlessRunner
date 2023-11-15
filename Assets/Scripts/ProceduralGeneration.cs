using System;
using System.Collections;
using System.Collections.Generic;
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
 *  15.11.2023   FM  created
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

    private List<GameObject> m_templateList;
    private float m_playerPositionZ;
    private const float c_renderDistance = 40f;

    private GameObject m_nextTemplateToSpawn;
    private Vector3 m_nextTemplateSpawnPosition;
    private float m_nextTemplateScale;
    private bool m_nextTemplateSpawned = false;
    private float m_lastTemplateScale;

    private int m_templateCounter = 0;

    private float m_templateLengthToSpeedRatio;
    private float m_speedModifier = 1f;

    private void Start()
    {
        GenerateInitialTemplates();
    }


    private void Update()
    {
        //Update Player Speed
        m_playerPositionZ = playerRef.transform.position.z;
        //Debug.Log(m_nextTemplateSpawnPosition + " " + m_playerPositionZ);

        CalculateNextTemplateValues();

        if (m_nextTemplateSpawnPosition.z - m_playerPositionZ < c_renderDistance && !m_nextTemplateSpawned)
        {        
            //Calculate next Template starting location
            m_nextTemplateSpawnPosition = CalculateNextTemplateSpawnPosition();
            //Spawn next Template
            //SpawnNextTemplate(m_nextTemplateToSpawn, m_nextTemplateSpawnPosition, m_nextTemplateScale);
            m_nextTemplateSpawned = true;
            //UpdatePlayerSpeedModifier();
        }
        if(m_playerPositionZ == m_nextTemplateSpawnPosition.z)
        {
            m_nextTemplateSpawned = false;
        }
    }
    private void GenerateInitialTemplates()
    {
        Instantiate(startingTemplateRef);
        m_templateList.Add(startingTemplateRef);
        CalculateNextTemplateValues();
    }

    private void CalculateNextTemplateValues()
    {
        m_templateLengthToSpeedRatio = playerRef.GetComponent<PlayerController>().GetVerticalSpeed() / startingTemplateRef.transform.GetChild(0).GetChild(0).localScale.z;
        //Debug.Log( " vertical speed: " + playerRef.GetComponent<PlayerController>().GetVerticalSpeed() + "playerPosZ: " + playerRef.transform.position.z + " ratio:" + m_templateLengthToSpeedRatio);
        m_lastTemplateScale = m_templateList[m_templateCounter].transform.localScale.z;
        m_nextTemplateScale =  + m_lastTemplateScale * m_templateLengthToSpeedRatio;   //scale faktor * speed faktor (gerundet)
        m_nextTemplateSpawnPosition = m_lastTemplateScale + m_lastTemplateSpawnPosition;
    }

    private void SpawnNextTemplate(GameObject _templateToSpawn, Vector3 _templateSpawnPosition, Vector3 _templateScale)
    {
        throw new NotImplementedException();
    }

    private Vector3 CalculateNextTemplateSpawnPosition()
    {
        return new Vector3(0,0,0);
    }

    private void UpdatePlayerSpeedModifier()
    {
        m_speedModifier -= 0.05f;
    }

    public float GetSpeedModifier()
    {
        return m_speedModifier;
    }
}
