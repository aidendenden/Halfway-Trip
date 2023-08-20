using UnityEngine;

public class MirrorObjects : MonoBehaviour
{
    public Transform object1; // 第一个物体
    public Transform object2; // 第二个物体

    void Update()
    {
        // 使两个物体的位置始终根据x=0平面呈镜像对称
        object2.position = new Vector3(-object1.position.x, object1.position.y, object1.position.z);
    }
}