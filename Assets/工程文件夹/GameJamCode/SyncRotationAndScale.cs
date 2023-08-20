using UnityEngine;

public class SyncRotationAndScale : MonoBehaviour
{
    public Transform target; // Ŀ������
    public float maxDistance = 10f; // ������
    public float minScale = 0.5f; // ��С���ű���
    public float scaleRatio = 0.5f; // ���ű�������


  
    private Vector3 initialScale; // ��ʼ����

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // ������������֮��ľ���
        float distance = Vector3.Distance(transform.position, target.position);

        // ������ת�Ƕ�
        transform.rotation = target.rotation;

        // �������ű���
        float scale = Mathf.Clamp(1f - distance / maxDistance, minScale, 1f);
        float scaledRatio = Mathf.Pow(scaleRatio, distance / maxDistance);
        transform.localScale = initialScale * scale * scaledRatio;



        Vector3 targetPosition = target.position;

        // ������yzƽ���ϵľ���Գ�λ��
        Vector3 mirroredPosition = new Vector3(gameObject.transform.position.x, targetPosition.y, targetPosition.z);

        // ���������λ��Ϊ����Գ�λ��
        transform.position = mirroredPosition;
    }


    

    
}
