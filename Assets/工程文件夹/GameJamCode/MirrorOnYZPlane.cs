using UnityEngine;

public class MirrorOnYZPlane : MonoBehaviour
{
    public Transform target; // 目标物体

    void Update()
    {
        // 获取目标物体的位置
        Vector3 targetPosition = target.position;

        // 计算在yz平面上的镜像对称位置
        Vector3 mirroredPosition = new Vector3(gameObject.transform.position.x, targetPosition.y, targetPosition.z);

        // 设置物体的位置为镜像对称位置
        transform.position = mirroredPosition;
    }
}
