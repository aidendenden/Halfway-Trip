using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCurr : MonoBehaviour
{

    public bool isUI = false;
    // Start is called before the first frame update
    void Start()
    {

        // �������������Ļ�м�
        Cursor.lockState = CursorLockMode.Locked;
        // ���ع��
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void checkUI()
    {
        if (isUI)
        {
            // �������������Ļ�м�
            Cursor.lockState = CursorLockMode.Confined;
            // ���ع��
            Cursor.visible = true;
        }

        if (!isUI)
        {
            // �������������Ļ�м�
            Cursor.lockState = CursorLockMode.Locked;
            // ���ع��
            Cursor.visible = false;
        }

    }
}
