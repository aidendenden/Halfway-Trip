using UnityEngine;

public class MirrorOnYZPlane : MonoBehaviour
{
    public Transform target; // Ŀ������

    void Update()
    {
        // ��ȡĿ�������λ��
        Vector3 targetPosition = target.position;

        // ������yzƽ���ϵľ���Գ�λ��
        Vector3 mirroredPosition = new Vector3(gameObject.transform.position.x, targetPosition.y, targetPosition.z);

        // ���������λ��Ϊ����Գ�λ��
        transform.position = mirroredPosition;
    }
}
