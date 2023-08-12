using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 工程文件夹.Code;

public class ForItem : MonoBehaviour
{
    public int ItemKind = 0;
    public SwitchItems switchItems;
    public CameraSwitch cameraSwitch;
    public MangerManger mangerManger;
    public VirtualSnapshotScript virtualSnapshot;

    private void OnEnable()
    {
        switchItems.currentItem = ItemKind;

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(ItemKind == 2)
            {
                usingCameraLeft();
            }

            if (ItemKind == 3)
            {
                usingBottonLeft();
            }

        }
        if (Input.GetMouseButtonDown(1))
        {

            if (ItemKind == 2)
            {
                usingCameraRight();
            }


            if (ItemKind == 3)
            {
                usingBottonRight();
            }


        }
    }

    void usingBottonLeft()
    {

        if (mangerManger.CameraMode==0) {
            cameraSwitch.SwitchCamera(1);
        }
        
    }
    void usingBottonRight()
    {

        if (mangerManger.CameraMode == 1)
        {
            cameraSwitch.SwitchCamera(0);
        }

    }


    void usingCameraRight()
    {

        if (mangerManger.CameraMode == 0 && virtualSnapshot.cameraOn)
        {
            GameEventManager.Instance.Triggered("CameraRight", transform);
        }

    }
    void usingCameraLeft()
    {

        if (mangerManger.CameraMode == 0&&virtualSnapshot.cameraOff)
        {
            GameEventManager.Instance.Triggered("CameraLeft", transform);
        }

    }
}
