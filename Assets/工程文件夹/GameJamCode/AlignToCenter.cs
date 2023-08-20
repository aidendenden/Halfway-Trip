using UnityEngine;

public class AlignToCenter : MonoBehaviour
{
    public Transform target1; // 第一个目标物体
    public Transform target2; // 第二个目标物体

    void Update()
    {
        // 计算两个物体的中心点
        Vector3 center = (target1.position + target2.position) / 2f;

        // 设置第三个物体的位置为中心点的位置
        transform.position = center;
    }
}