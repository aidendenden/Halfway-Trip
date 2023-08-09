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
        cube.AddComponent<Rigidbody>();
        var position = playerObject.transform.position;
        cube.transform.position = new Vector3(position.x+0.5f,position.y+0.3f,position.z);
    }
}
