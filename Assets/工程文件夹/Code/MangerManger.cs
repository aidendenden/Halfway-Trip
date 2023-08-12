using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangerManger : MonoBehaviour
{
    public int CameraMode;//相机状态，0是在场景中，1是在相册中
    public int ItemMode;//手持物品，0是空，1是手，2是相机，3是瓶子;

  
    public CameraSwitch cameraSwitch;
    public SwitchItems switchItems;

    void Update()
    {
        CheckCamera();
        ItemMode = switchItems.currentItem;
    }

    void CheckCamera()
    {
       
        CameraMode = cameraSwitch.CamaerNow;
       // Debug.Log(CameraMode);
    }
}
