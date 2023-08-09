using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffAndOn : MonoBehaviour
{
   public void ActiveOn()
    {
        gameObject.SetActive(true);
        Debug.Log("ON!");
    }
    public void ActiveOff()
    {
        gameObject.SetActive(false);
        Debug.Log("OFF!");
        GameObject playerObject = GameObject.FindWithTag("Player");
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer cubeRenderer = cube.GetComponent<Renderer>();

        // 生成随机颜色
        Color randomColor = new Color(Random.value, Random.value, Random.value);

        // 设置立方体的材质颜色
        cubeRenderer.material.color = randomColor;
        cube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        var position = playerObject.transform.position;

        Vector3 originalPosition = position;

        // 前方方向
        Vector3 forwardDirection = playerObject.transform.forward;

        // 距离
        float distance = 1f;

        // 计算正前方位置
        Vector3 positionInFront = originalPosition + forwardDirection * distance;
        
        cube.transform.position = new Vector3(positionInFront.x,positionInFront.y+0.3f,positionInFront.z);
        cube.tag = "Detectable";
        int layerIndex = LayerMask.NameToLayer("DetectableLayer");
        cube.layer = layerIndex;
        cube.AddComponent<Rigidbody>();
    }
}
