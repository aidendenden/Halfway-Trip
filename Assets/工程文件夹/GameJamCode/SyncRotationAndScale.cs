using UnityEngine;

public class SyncRotationAndScale : MonoBehaviour
{
    public Transform target; // Ŀ������
    public float maxDistance = 10f; // ������
    public float minDistance = 1f; // ��ʼ���ŵ���С����
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
        float changeDistance;

        if (distance >= minDistance)
        {
            //�����볬����С����(���ӵĺ��)֮��ʼ����
            changeDistance = distance - minDistance;
            // �������ű���
            float scale = Mathf.Clamp(1f - changeDistance / maxDistance, minScale, 1f);
            float scaledRatio = Mathf.Pow(scaleRatio, changeDistance / maxDistance);
            transform.localScale = initialScale * scale * scaledRatio;
        }

        // ������ת�Ƕ�
        transform.rotation = target.rotation;
        Vector3 targetPosition = target.position;

        // ������yzƽ���ϵľ���Գ�λ��
        Vector3 mirroredPosition = new Vector3(gameObject.transform.position.x, targetPosition.y, targetPosition.z);

        // ���������λ��Ϊ����Գ�λ��
        transform.position = mirroredPosition;
    }


    

    
}
