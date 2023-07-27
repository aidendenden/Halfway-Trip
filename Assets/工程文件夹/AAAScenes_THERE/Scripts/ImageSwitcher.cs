
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ImageSwitcher : MonoBehaviour
{
    
    public Sprite[] spriteS;
    


    private Image image;

    private void Start()
    {
       
        image = gameObject.GetComponent<Image>();
        // ��ʼ��ͼ��
        SwitchImage(0);
        
    }

    private void Update()
    {
        
    }

    public void SwitchImage(int toolKind)
    {
        image.sprite = spriteS[toolKind];
    }
}
