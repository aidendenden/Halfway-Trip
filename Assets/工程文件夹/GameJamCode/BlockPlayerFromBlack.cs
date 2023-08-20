using UnityEngine;

public class BlockPlayerFromBlack : MonoBehaviour
{
    public Material material; // 物体的材质
    public LayerMask blockingLayer; // 阻止玩家移动的层级

    void Start()
    {
        // 创建一个临时的RenderTexture来渲染物体的材质
        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        Graphics.Blit(null, renderTexture, material);

        // 将RenderTexture的内容读取到一个Texture2D中
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
        renderTexture.Release();

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixelColor = texture.GetPixel(x, y);

                // 如果颜色是黑色
                if (pixelColor.r < 0.1f && pixelColor.g < 0.1f && pixelColor.b < 0.1f)
                {
                    // 在黑色区域上添加一个Collider
                    GameObject blockingObject = new GameObject("Blocking Object");
                    blockingObject.transform.parent = transform;
                    blockingObject.transform.localPosition = new Vector3(x - texture.width / 2f + 0.5f, y - texture.height / 2f + 0.5f, 0f);
                    blockingObject.layer = blockingLayer;
                    blockingObject.AddComponent<BoxCollider2D>();
                }
            }
        }
    }
}
