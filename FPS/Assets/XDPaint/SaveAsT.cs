using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint;
using UnityEngine.UI;

public class SaveAsT : MonoBehaviour
{
    public PatintForK pk;
    public PaintManager p;
    public Texture t;
    public Color c;

    private bool isopen = false;
    // Start is called before the first frame update
    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            S();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
           
            
            
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log(p.LayersController.ActiveLayerIndex);
            
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            ActivateButton();

        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            CC();

        }

        if (!isopen)
        {
            LoadL();
            pk.ClearTool();
            isopen = true;
        }
    }



    

    public void ActivateButton()
    {
        p.Brush.SetTexture(t, true, false);
    }
    public void CC()
    {
        p.Brush.SetColor(c);
    }



    private void OnEnable()
    {
       
        L();
        

    }

    private void OnDisable()
    {
        isopen = false;
        S();
    }

    public void S()
    {
        SaveResultToFile(p, "AA.png");
        Debug.Log("aa");
    }
    public void L()
    {
        LoadResultTextureFromFile(p, "PaperOneA.png");
        Debug.Log("bb");
    }

    public void LoadL()
    {
        var filePath = System.IO.Path.Combine(Application.persistentDataPath, "AA.png");
        var textureData = System.IO.File.ReadAllBytes(filePath);
        var texture = new Texture2D(1, 1);
        texture.LoadImage(textureData);
        p.LayersController.AddNewLayer("BB", texture);
    }


    public void LoadResultTextureFromFile(PaintManager paintManager, string fileName)//º”‘ÿ
    {
        var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        Debug.Log(filePath);
        var textureData = System.IO.File.ReadAllBytes(filePath);
        var texture = new Texture2D(1, 1);
        texture.LoadImage(textureData);
        var shaderTextureName = paintManager.Material.ShaderTextureName;
        var previousTexture = paintManager.Material.SourceMaterial.GetTexture(shaderTextureName);
        paintManager.Material.SourceMaterial.SetTexture(shaderTextureName, texture);
        var spriteRenderer = paintManager.ObjectForPainting.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
        }
      
        paintManager.CopySourceTextureToLayer = true;
        //copy texture to background layer:
        //paintManager.UseSourceTextureAsBackground = true;
        paintManager.Init();
        paintManager.Material.SourceMaterial.SetTexture(shaderTextureName, previousTexture);
    }
    private void SaveResultToFile(PaintManager paintManager, string fileName)//±£¥Ê
    {
        var texture2D = paintManager.GetResultTexture();
        var pngData = texture2D.EncodeToPNG();
        if (pngData != null)
        {
            var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            System.IO.File.WriteAllBytes(filePath, pngData);
            Debug.Log(filePath);
        }
        Destroy(texture2D);
    }
}
