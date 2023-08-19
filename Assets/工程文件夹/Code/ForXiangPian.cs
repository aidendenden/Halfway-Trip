using UnityEngine;
using System.IO;

public class ForXiangPian : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        LoadSprite();
    }

    private void LoadSprite()
    {
        string objectName = gameObject.name;
        string imagePath = Application.dataPath + "/snapshots/" + objectName + ".png";
        // Debug.Log(imagePath);

        Texture2D Tex2D;
        byte[] FileData;

        FileData = File.ReadAllBytes(imagePath);
        Tex2D = new Texture2D(1920, 1080);
        Tex2D.LoadImage(FileData);
        Texture2D SpriteTexture = Tex2D;

        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), 200);


        if (NewSprite != null)
        {
            spriteRenderer.sprite = NewSprite;
        }
        else
        {
            Debug.LogWarning("No sprite found for object: " + objectName);
        }
    }
}
