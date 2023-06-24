using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTrack : MonoBehaviour
{
    public Camera playerCamera;

    private bool needTrack;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (needTrack)
        {
            var transform1 = playerCamera.transform;
            var transform2 = transform;
            transform2.position=transform1.position;
            transform2.rotation = transform1.rotation;
        }
      
    }

    private void OnEnable()
    {
        needTrack = true;
    }

    private void OnDisable()
    {
        needTrack = false;
    }
}
