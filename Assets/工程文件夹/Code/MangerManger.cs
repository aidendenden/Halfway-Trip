using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MangerManger : MonoBehaviour
{
    public int CameraMode;//���״̬��0���ڳ����У�1���������
    public int ItemMode;//�ֳ���Ʒ��0�ǿգ�1���֣�2�������3��ƿ��;

  
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
