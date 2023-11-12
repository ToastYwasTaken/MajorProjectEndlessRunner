using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/******************************************************************************
 * Project: MajorProjectEndlessRunner
 * File: CameraController.cs
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
 *  18.08.2023   FM  created
 *  
 *****************************************************************************/
public class CameraController : MonoBehaviour
{
    private Transform m_playerRef;

    // Start is called before the first frame update
    void Start()
    {
        m_playerRef = transform.gameObject.GetComponentInParent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(m_playerRef);
    }
}
