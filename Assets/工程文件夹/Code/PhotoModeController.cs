using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoModeController : MonoBehaviour
{

    [Header("Camera Of The Player, Default : Main Camera")]
    public Camera PlayerCamera;
    [Space]

    [Header("Camera Of The PhotoMode")]
    public Camera PhotoCamera;
    [Space]



    [Header("Resolution Of Photo Camera")]
    public Vector2 Resolution=new Vector2(1920,1080);
    [Space]

    public GameObject PhotoCanvas;
    [Space]

    public GameObject PhotoTexture;

    public GameObject PhotoGalery;

    [Tooltip("Key To Open Photo Galery")]
    public KeyCode PhotoGaleryKey = KeyCode.G;

    [Tooltip("Key To Start PhotoMode")]
    public KeyCode PhotoModeKey = KeyCode.F;
    [Tooltip("Key Of Take Photo In PhotoMode")]
    public KeyCode TakePhotoKey = KeyCode.Mouse0;
    // Start is called before the first frame update

    [Tooltip("Time Of Displayed Picture Taken")]
    public float WaitAfterTaken = 0.5f;


    [Header("Path of the captured images. Default:Persistent data path of application")]
    public string SavePath;


    public GameObject TakePhotoAnimObj;

    [Header("Zoom Settings")]
    public float zoomLevel;
    public float scrollsensitivity = 1;
    public float speed = 30;
    public float maxZoom = 90f;
    public float minZoom = 10f;

    [Space]
    [Space]

    private bool isEnablePhotoMode = false;
    private bool isEnablePhotoGalery = false;
    private float zoomPosition;




    public static string GaleryPath;
    public static bool isOpenedGallery = false;
    
    [Tooltip("拍摄目标")]
    public GameObject cameraTarget;

    // If the target is captured on screen, but outside this range, it does not count as a target capture.
    [Tooltip("Total distance (percent of screen size) allowed from center of camera viewfinder that still counts as target capture. E.g. 0.5 means target must be in center 50% of screen width and height.")]
    public float cameraTargetDistanceTolerance = 0.5f;
    //private bool cameraTargetCapturedInLastSnapshot=false;
    private Vector3 cameraTargetScreenPosition=Vector3.zero;
    private float currentZoom;
    private float targetZoom;
    private float startZoom;
    private float previousZoom;


    private void Awake()
    {
        startZoom = PhotoCamera.fieldOfView;
        currentZoom = startZoom;
        targetZoom = startZoom;
        previousZoom = startZoom;
    }


    void Start()
    {
        TakePhotoAnimObj.SetActive(false);
        PhotoCamera.gameObject.SetActive(false);
        PhotoCanvas.gameObject.SetActive(false);
        PhotoCamera.transform.parent.gameObject.SetActive(true);

        if (PlayerCamera == null)
        {
            PlayerCamera = Camera.main;
        }
        PhotoTexture.GetComponent<RawImage>().mainTexture.width = (int)Resolution.x;
        PhotoTexture.GetComponent<RawImage>().mainTexture.height = (int)Resolution.y;


        if (SavePath=="")
        {
            SavePath = Application.persistentDataPath + "/PhotoGalery";
        }

        GaleryPath = SavePath;

    }
    void Update()
    {
        if (Input.GetKeyDown(PhotoModeKey))
        {
            isEnablePhotoMode = !isEnablePhotoMode;
        }
        if (Input.GetKeyDown(PhotoGaleryKey))
        {
            isEnablePhotoGalery = !isEnablePhotoGalery;
        }

        if (isEnablePhotoMode&& !isOpenedGallery)
        {
            enablePhotoMode();
        }
        else
        {
            disablePhotoMode();
        }


        if (isEnablePhotoGalery)
        {
            isOpenedGallery = true;
            PhotoGalery.SetActive(true);
        }
        else
        {
            isOpenedGallery = false;
            PhotoGalery.SetActive(false);
        }


    }

    void enablePhotoMode()
    {
        PhotoCamera.gameObject.SetActive(true);
        PhotoCanvas.gameObject.SetActive(true);
        PhotoCamera.transform.parent.eulerAngles = PlayerCamera.transform.eulerAngles;
        PhotoCamera.transform.parent.position = PlayerCamera.transform.position;

        ZoomControl();
        TakePhotoControl();

    }



    void disablePhotoMode()
    {
        targetZoom = previousZoom;
        PhotoCamera.gameObject.SetActive(false);
        PhotoCanvas.gameObject.SetActive(false);
    }



    void ZoomControl()
    {
        previousZoom = currentZoom;
        targetZoom = startZoom;
        
        
        // if(Input.GetKey(KeyCode.Equals)){
        //     targetZoom = Mathf.Max(targetZoom - 60.0f*Time.deltaTime, minZoom);
        //     currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);
        //     PhotoCamera.fieldOfView += currentZoom;
        // }
        // if(Input.GetKey(KeyCode.Minus)){
        //     targetZoom = Mathf.Min(targetZoom + 60.0f*Time.deltaTime, maxZoom);
        //     currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);
        //     PhotoCamera.fieldOfView += currentZoom;
        // }

        // For mouse wheel

        if(Input.mouseScrollDelta.y > 0){
            targetZoom = Mathf.Max(targetZoom - 500.0f*Time.deltaTime, minZoom);
            currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);
            PhotoCamera.fieldOfView = currentZoom;
        }
        if(Input.mouseScrollDelta.y < 0){
            targetZoom = Mathf.Min(targetZoom + 500.0f*Time.deltaTime, maxZoom);
            currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);
            PhotoCamera.fieldOfView = currentZoom;
        }

     
        
        // zoomLevel += Input.mouseScrollDelta.y * scrollsensitivity;
        // zoomLevel = Mathf.Clamp(zoomLevel, 0, maxZoom);
        // targetZoom = Mathf.Max(zoomLevel - 500.0f*Time.deltaTime, minZoom);
        // currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);
        // PhotoCamera.fieldOfView = currentZoom;
        
        // ClipCheck();
        // zoomPosition = Mathf.MoveTowards(zoomPosition, zoomLevel, speed * Time.deltaTime);
        // PhotoCamera.transform.position = PhotoCamera.transform.parent.position + (PhotoCamera.transform.forward * zoomPosition);

    }


    // void ClipCheck()
    // {
    //     Ray ray = new Ray(PhotoCamera.transform.parent.position, PhotoCamera.transform.forward);
    //     if (Physics.SphereCast(ray, 3, out RaycastHit hit, maxZoom))
    //     {
    //         if (hit.distance < zoomLevel + 3)
    //         {
    //             zoomLevel = Mathf.Clamp( hit.distance - 3, 0, zoomLevel);
    //         }
    //     }
    // }




    void TakePhotoControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(TakePhotoAnim());
        }
    }

    IEnumerator TakePhotoAnim()
    {
        float startSize = 50;
        float speedAnim = 400f*Time.deltaTime;

        TakePhotoAnimObj.transform.localScale = Vector3.one * 50;
        TakePhotoAnimObj.SetActive(true);
        
        // Target Capture stuff
        if(cameraTarget){
            
            RaycastHit hitTest;
            Vector3 rayCastDirection = cameraTarget.transform.position - PhotoCamera.transform.position;
             //cameraTargetCapturedInLastSnapshot = false;

            // Do line of sight test first to ensure it's possible to capture target in the snapshot
            if(Physics.Raycast(PhotoCamera.transform.position, rayCastDirection, out hitTest)){

                // If we can see it, check that it's within the viewfinder according to specified percentage
                if(hitTest.collider.gameObject == cameraTarget){

                    cameraTargetScreenPosition = PhotoCamera.WorldToScreenPoint(cameraTarget.transform.position);
                    Vector2 cameraTargetDelta = Vector2.zero;
                    cameraTargetDelta.x = Mathf.Abs(Screen.width/2 - cameraTargetScreenPosition.x);
                    cameraTargetDelta.y = Mathf.Abs(Screen.height/2 - cameraTargetScreenPosition.y);

                    if((cameraTargetDelta.x/Screen.width) <= cameraTargetDistanceTolerance && (cameraTargetDelta.y/Screen.height) <= cameraTargetDistanceTolerance){
                        //cameraTargetCapturedInLastSnapshot = true;
                    }

                }

            }

        }

        int i = 0;
        while (i < (int)(startSize / speedAnim))
        {
            ++i;
            if (TakePhotoAnimObj.transform.localScale.x > 1)
            {
                TakePhotoAnimObj.transform.localScale -= Vector3.one * speedAnim;
            }
            yield return new WaitForSeconds(0.0001f);
        }

        PhotoCamera.GetComponent<Camera>().enabled = false;

        i = 0;
        while (i < (int)(startSize / speedAnim))
        {
            ++i;
            if (TakePhotoAnimObj.transform.localScale.x < 50)
            {
                TakePhotoAnimObj.transform.localScale += Vector3.one * speedAnim;
            }
            yield return new WaitForSeconds(0.0001f);
        }
        TakePhotoAnimObj.SetActive(false);

        StartCoroutine(SaveTexture((RenderTexture)PhotoTexture.GetComponent<RawImage>().mainTexture));
    

        yield return new WaitForSeconds(WaitAfterTaken);


        PhotoCamera.GetComponent<Camera>().enabled = true;


    }
    // Update is called once per frame

    IEnumerator SaveTexture(RenderTexture texture)
    {
        Texture2D tex2D = toTexture2D(texture);

        byte[] bytes = tex2D.EncodeToJPG(100); ;
        yield return null;
        var dirPath = SavePath;
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
        string lastPath = dirPath + "/Photo_" + cur_time + ".jpg";
        System.IO.File.WriteAllBytesAsync(lastPath, bytes);
    }


    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }






}

