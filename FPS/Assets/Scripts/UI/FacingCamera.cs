﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    Transform[] childs;
    // Start is called before the first frame update
    void Start()
    {
        childs = new Transform[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            childs[i] = transform.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
       Input.GetAxis("Mouse ScrollWheel")
    }
}
