using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
