using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
   
    void Update()
    {
        // ��ȡ��굱ǰλ��
        Vector3 mousePosition = Input.mousePosition;
      

        // �����λ��ת��Ϊ����ռ�����
        

        // ��������λ��Ϊ���λ��
        transform.position = mousePosition;
    }
}
