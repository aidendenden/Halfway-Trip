using UnityEngine;

public class SyncRotationAndScale : MonoBehaviour
{
    public Transform target; // 目标物体
    public float maxDistance = 10f; // 最大距离
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

        // 计算旋转角度
        transform.rotation = target.rotation;

        // 计算缩放比例
        float scale = Mathf.Clamp(1f - distance / maxDistance, minScale, 1f);
        float scaledRatio = Mathf.Pow(scaleRatio, distance / maxDistance);
        transform.localScale = initialScale * scale * scaledRatio;

       
    }


    

    
}
