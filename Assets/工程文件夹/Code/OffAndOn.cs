using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class OffAndOn : MonoBehaviour
{
    public GameObject[] ObjectS;

    public void ActiveOn()
    {
        gameObject.SetActive(true);
        Debug.Log("ON!");
    }
    public void ActiveOff()
    {
        gameObject.SetActive(false);
        Debug.Log("OFF!");
        
    }

    public void SpawnCube()
    {
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

        cube.transform.position = new Vector3(positionInFront.x, positionInFront.y + 0.3f, positionInFront.z);
        cube.tag = "Detectable";
        int layerIndex = LayerMask.NameToLayer("DetectableLayer");
        cube.layer = layerIndex;
        cube.AddComponent<Rigidbody>();
    }

    public void SpawnPureCubeObjectToPlayer()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        Vector3 pr = new Vector3(Random.Range(-3, 3), Random.Range(10, 15), Random.Range(-3, 3));
        GameObject obj = Instantiate(ObjectS[0], playerObject.transform.position + pr, Quaternion.identity);
        ForBlock _forBlock = obj.GetComponent<ForBlock>();
        _forBlock.isCanMove = false;
        
    }

    public void InspectionPhoto()
    {
        GameObject PhotoInspectionObject = GameObject.FindWithTag("PhotoInspect");

        if (!PhotoInspectionObject)
        {
            return;
        }
        
        PhotoInspectionObject.SetActive(true);
        SpriteRenderer showPhoto = PhotoInspectionObject.GetComponent<PhotoInspection>().photo;
        SpriteRenderer photo = this.transform.gameObject.GetComponent<SpriteRenderer>();
        TextMeshPro textMesh =  PhotoInspectionObject.GetComponent<PhotoInspection>().textMesh;
        
        showPhoto.sprite = photo.sprite;
        textMesh.text = photo.name;
        
        
        // string imagePath = Application.dataPath + "/snapshots/" + objectName + ".png";
        //
        // Texture2D Tex2D;
        // byte[] FileData;
        //
        // FileData = File.ReadAllBytes(imagePath);
        // Tex2D = new Texture2D(1920, 1080);
        // Tex2D.LoadImage(FileData);
        // Texture2D SpriteTexture = Tex2D;
        //
        // Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), 200);
        //
        //
        // if (NewSprite != null)
        // {
        //     photo.sprite = NewSprite;
        // }
        // else
        // {
        //     Debug.LogWarning("No sprite found for object: " + objectName);
        // }
        
    }
}
