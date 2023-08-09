using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCurr : MonoBehaviour
{

    public bool isUI = false;
    // Start is called before the first frame update
    void Start()
    {

        // 将光标锁定到屏幕中间
        Cursor.lockState = CursorLockMode.Locked;
        // 隐藏光标
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
            // 将光标锁定到屏幕中间
            Cursor.lockState = CursorLockMode.Confined;
            // 隐藏光标
            Cursor.visible = true;
        }

        if (!isUI)
        {
            // 将光标锁定到屏幕中间
            Cursor.lockState = CursorLockMode.Locked;
            // 隐藏光标
            Cursor.visible = false;
        }

    }
}
