using UnityEngine;

public class MirrorObjects : MonoBehaviour
{
    public Transform object1; // ��һ������
    public Transform object2; // �ڶ�������

    void Update()
    {
        // ʹ���������λ��ʼ�ո���x=0ƽ��ʾ���Գ�
        object2.position = new Vector3(-object1.position.x, object1.position.y, object1.position.z);
    }
}