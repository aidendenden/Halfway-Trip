using UnityEngine;

public class SyncRotationAndScale : MonoBehaviour
{
    public Transform target; // 目标物体
    public float maxDistance = 10f; // 最大距离
    public float minDistance = 1f; // 开始缩放的最小距离
    public float minScale = 0.5f; // 最小缩放比例
    public float scaleRatio = 0.5f; // 缩放比例参数


  
    private Vector3 initialScale; // 初始缩放

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 计算两个物体之间的距离
        float distance = Vector3.Distance(transform.position, target.position);
        float changeDistance;

        if (distance >= minDistance)
        {
            //当距离超过最小距离(镜子的厚度)之后开始缩放
            changeDistance = distance - minDistance;
            // 计算缩放比例
            float scale = Mathf.Clamp(1f - changeDistance / maxDistance, minScale, 1f);
            float scaledRatio = Mathf.Pow(scaleRatio, changeDistance / maxDistance);
            transform.localScale = initialScale * scale * scaledRatio;
        }

        // 计算旋转角度
        transform.rotation = target.rotation;
        Vector3 targetPosition = target.position;

        // 计算在yz平面上的镜像对称位置
        Vector3 mirroredPosition = new Vector3(gameObject.transform.position.x, targetPosition.y, targetPosition.z);

        // 设置物体的位置为镜像对称位置
        transform.position = mirroredPosition;
    }


    

    
}
