using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GaleryController : MonoBehaviour
{

    public GameObject imgsmall;
    public GameObject bigphoto;

    FileInfo[] fileInfos;



    List<Sprite> allSprites = new List<Sprite>();

    int imgNum = -1;


    private GameObject lastSelected;
    // Start is called before the first frame update
    void OnEnable()
    {

        RefreshGalery();
      
    }

    void RefreshGalery()
    {
        imgNum = -1;
        allSprites = new List<Sprite>();
        StartCoroutine(loadImages());
        Cursor.lockState = CursorLockMode.None; // keep confined in the game window
        Cursor.visible = true;
        imgsmall.name = "-1";
        imgsmall.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width / 11, Screen.height / 11);
        clearGalleryItems();
    }
    void clearGalleryItems()
    {
        GameObject content = imgsmall.transform.parent.gameObject;
        Transform[] allChildren = content.GetComponentsInChildren<Transform>();
        int count = allChildren.Length;
        for(int i=0;i< count;++i)
        {
            if (allChildren[i].gameObject.name!= imgsmall.name&& allChildren[i].gameObject.name != content.name) {
            
                Destroy(allChildren[i].gameObject);
            }
         
        }
    }
    IEnumerator loadImages()
    {
        yield return null;
        var dirPath = PhotoModeController.GaleryPath;
        var info = new DirectoryInfo(dirPath);
        fileInfos = info.GetFiles("*.jpg", SearchOption.AllDirectories);
        bool isfirst = true;

        bigphoto.GetComponent<Image>().sprite =null;
        bigphoto.SetActive(false);
        foreach (FileInfo fileInfo in fileInfos)
        {

            yield return null;
            string lastPath = fileInfo.FullName;
            Texture2D Tex2D;
            byte[] FileData;


            FileData = File.ReadAllBytes(lastPath);
            Tex2D = new Texture2D(1366, 768);
            Tex2D.LoadImage(FileData);       // Load the imagedata into the texture (size is set automatically)
            Texture2D SpriteTexture = Tex2D;

            Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0),1);
            allSprites.Add(NewSprite);

            GameObject imgsmallCopy = Instantiate(imgsmall, imgsmall.transform.position, imgsmall.transform.rotation);
            ++imgNum;
            imgsmallCopy.name = imgNum+"";
            imgsmallCopy.transform.parent = imgsmall.transform.parent;
            imgsmallCopy.GetComponent<Image>().sprite = NewSprite;
            imgsmallCopy.SetActive(true);

            if (isfirst)
            {
                isfirst = false;
                OnClickImage(imgsmallCopy);
            }
           
        }
      



    }
    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnClickImage(GameObject clickedObj)
    {
        if (lastSelected!=null)
        {
            lastSelected.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
        clickedObj.GetComponent<Image>().color = new Color(1, 1, 1, 1f);
        lastSelected = clickedObj;
        bigphoto.GetComponent<Image>().sprite = allSprites[int.Parse(clickedObj.name)];
        bigphoto.SetActive(true);
    }




    public void OnClickDeleteBtn()
    {
        if (lastSelected==null)
        {
            return;
        }
        int photoId = int.Parse(lastSelected.name);
        string delPath = fileInfos[photoId].FullName;
        if (File.Exists(delPath))
        {
            File.Delete(delPath);
        }
        RefreshGalery();
    }
}
