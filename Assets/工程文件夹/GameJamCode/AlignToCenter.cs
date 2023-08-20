using UnityEngine;

public class AlignToCenter : MonoBehaviour
{
    public Transform target1; // ��һ��Ŀ������
    public Transform target2; // �ڶ���Ŀ������

    void Update()
    {
        // ����������������ĵ�
        Vector3 center = (target1.position + target2.position) / 2f;

        // ���õ����������λ��Ϊ���ĵ��λ��
        transform.position = center;
    }
}