using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangerManger : MonoBehaviour
{
    public int CameraMode;//相机状态，0是在场景中，1是在相册中

  
    public CameraSwitch cameraSwitch;


    void Update()
    {
        CheckCamera();
    }

    void CheckCamera()
    {
       
        CameraMode = cameraSwitch.CamaerNow;
       // Debug.Log(CameraMode);
    }
}
