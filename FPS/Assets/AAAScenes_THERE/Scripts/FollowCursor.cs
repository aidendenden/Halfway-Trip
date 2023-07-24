using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
   
    void Update()
    {
        // 获取鼠标当前位置
        Vector3 mousePosition = Input.mousePosition;
      

        // 将鼠标位置转换为世界空间坐标
        

        // 设置物体位置为鼠标位置
        transform.position = mousePosition;
    }
}
