using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangerManger : MonoBehaviour
{
    public int CameraMode;//���״̬��0���ڳ����У�1���������

  
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
