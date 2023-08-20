using UnityEngine;

public class BlockPlayerFromBlack : MonoBehaviour
{
    public Material material; // ����Ĳ���
    public LayerMask blockingLayer; // ��ֹ����ƶ��Ĳ㼶

    void Start()
    {
        // ����һ����ʱ��RenderTexture����Ⱦ����Ĳ���
        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        Graphics.Blit(null, renderTexture, material);

        // ��RenderTexture�����ݶ�ȡ��һ��Texture2D��
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

                // �����ɫ�Ǻ�ɫ
                if (pixelColor.r < 0.1f && pixelColor.g < 0.1f && pixelColor.b < 0.1f)
                {
                    // �ں�ɫ���������һ��Collider
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
