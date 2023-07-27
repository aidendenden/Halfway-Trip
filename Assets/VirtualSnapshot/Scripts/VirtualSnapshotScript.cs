using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;


//
// Virtual Snapshot Script
// This script controls all camera behaviour
//
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class VirtualSnapshotScript : MonoBehaviour {

    [Tooltip("The Camera from which the snapshots will be taken (e.g. FPS Controller's Main Camera")]
    public Camera snapshotCamera;
    [Tooltip("The actual snapshot image size you want to capture.")]
    public int snapshotWidth = 1280;
    public int snapshotHeight = 720;
    [Tooltip("Toggle Zoom Capability On and Off. Recommended that you do not change this at runtime.")]
    public bool zoomEnabled = true;
    [Tooltip("Min/Max zoom levels for the camera's Field of View")]
    public float maxZoom = 90.0f;
    public float minZoom = 10.0f;
    [Tooltip("Toggle camera flash On/Off")]
    public bool cameraFlashEnabled = true;
    [Tooltip("The 2 sprites for camera flash indicator icon (for on and off)")]
    public Sprite[] flashIndicatorIconSprites;
    [Tooltip("GUI Colors for battery/flash icons and texts")]
    public Color CameraUITextColor = new Color(0.59f,1.0f,0.0f);
    public Color NoMoreExposuresTextColor = new Color(1.0f,0.0f,0.0f);
    public Color BatteryIconColorFilled = new Color(0.59f,1.0f,0.0f);
    public Color BatteryIconColorLow = new Color(1.0f,0.94f,0.0f);
    public Color BatteryIconColorEmpty = new Color(1.0f,0.0f,0.0f);

    [Tooltip("The 5 sprites for battery level")]
    public Sprite[] batteryLevelSprites;
    private int batteryLevel = 5;
    private RectTransform batteryLevelIcon;

    [Tooltip("Toggle UI elements On/Off. When Off, corresponding UI elements are deleted on scene start.")]
    public bool showBatteryIndicator = true;
    public bool showFlashIndicator = true;
    public bool showShutterSpeed = true;
    public bool showAperture = true;
    public bool showLightMeter = true;
    public bool showISO = true;
    public bool showExposures = true;

    private RectTransform flashIndicatorIcon;
    private RectTransform apertureText;
    private RectTransform shutterSpeedText;
    private RectTransform ISOLabelText;
    private RectTransform ISOValueText;
    private RectTransform ExposuresLabelText;
    private RectTransform ExposuresRemainingText;
    private RectTransform lightMeter;
    private RectTransform shutter;
    private bool takingSnapshot = false;
    // Note: Minimum shutter can't be a true simulation as a duration less than 1 frame is not visible :)
    private float shutterDurationInSeconds = 0.2f;
    private float shutterTimeRemaining = 0.0f;

    [Tooltip("If you want to simulate a memory card or film roll, set this to True")]
    public bool limitExposureCount = false;
    [Tooltip("If limitExposureCount is true, this is how many snapshots can be taken")]
    public int snapshotLimit = 27;
    // Always default to 1, don't change this
    private int exposuresRemaining = 1;
    
    // This prevents I/O bit-bashing, gives time to let the photo be written to file
    [Tooltip("Minimum delay (in seconds) between snapshots. Recommend about a quarter second or greater. If you find your platform hardware misses some photos when taken in quick succession, increase this value.")]
    public float minimumSnapshotInterval = 0.25f;
    private float snapshotCountdown = 0.0f;

    [Tooltip("The sounds the camera makes during operation")]
    public AudioClip[] shutterSounds;
    public AudioClip flashTurnOn;
    public AudioClip flashTurnOff;
    public AudioClip cameraOn;
    public AudioClip cameraOff;
    public AudioClip cameraZoomLoop;
    public AudioClip shutterNoExposuresLeft;

    // Zoom and flash stuff
    private float startZoom = 0.0f;
    private float currentZoom = 0.0f;
    private float targetZoom = 0.0f;
    private float previousZoom = 0.0f;
    private Vector2 startSensitivity = Vector2.zero;
    private GameObject cameraFlash;

    // Camera Up/Down transition stuff
    private bool movingCameraUp = false;
    private bool movingCameraDown = false;
    private bool cameraUp = false;
    private RectTransform mainCameraCanvas;
    private RectTransform cameraTransitionImage;

    // File management stuff
    private string snapshotDirectory = "";
    private string fileExtension = ".png";

    // Target detection stuff
    [Tooltip("Object to track in the scene (optional)")]
    public GameObject cameraTarget;
    // If the target is captured on screen, but outside this range, it does not count as a target capture.
    [Tooltip("Total distance (percent of screen size) allowed from center of camera viewfinder that still counts as target capture. E.g. 0.5 means target must be in center 50% of screen width and height.")]
    public float cameraTargetDistanceTolerance = 0.5f;
    private Vector3 cameraTargetScreenPosition = Vector3.zero;
    private bool cameraTargetCapturedInLastSnapshot = false;

    private GameObject thePlayer = null;

    void Awake(){

        startZoom = snapshotCamera.fieldOfView;
        currentZoom = startZoom;
        targetZoom = startZoom;
        previousZoom = startZoom;

        RectTransform[] rtc = gameObject.GetComponentsInChildren<RectTransform>();
        foreach(RectTransform rt in rtc) {

            if(rt.transform.name == "Shutter"){
                shutter = rt;
            }else if(rt.transform.name == "SnapshotUICanvas"){
                mainCameraCanvas = rt;
            }else if(rt.transform.name == "TransitionImage"){
                cameraTransitionImage = rt;
            }else if(rt.transform.name == "BatteryLevel"){
                batteryLevelIcon = rt;
            }else if(rt.transform.name == "Aperture"){
                apertureText = rt;
            }else if(rt.transform.name == "ShutterSpeed"){
                shutterSpeedText = rt;
            }else if(rt.transform.name == "ISOLabel"){
                ISOLabelText = rt;
            }else if(rt.transform.name == "ISOValue"){
                ISOValueText = rt;
            }else if(rt.transform.name == "FlashIndicator"){
                flashIndicatorIcon = rt;
            }else if(rt.transform.name == "LightMeter"){
                lightMeter = rt;
            }else if(rt.transform.name == "ExposureLabel"){
                ExposuresLabelText = rt;
            }else if(rt.transform.name == "ExposuresRemaining"){
                ExposuresRemainingText = rt;
            }

        }

        // We check for presence to ensure prefab isn't broken
        if(showBatteryIndicator && !batteryLevelIcon){
            Debug.LogError("VirtualSnapshotScript:: Battery Level UI Image is missing");
        }
        if(showFlashIndicator && !flashIndicatorIcon){
            Debug.LogError("VirtualSnapshotScript:: Flash Indicator UI Image is missing");
        }
        if(showShutterSpeed && !shutterSpeedText){
            Debug.LogError("VirtualSnapshotScript:: Shutter Speed UI Text is missing");
        }
        if(showAperture && !apertureText){
            Debug.LogError("VirtualSnapshotScript:: Aperture UI Text is missing");
        }
        if(showLightMeter && !lightMeter){
            Debug.LogError("VirtualSnapshotScript:: Light Meter UI Image is missing");
        }
        if(showISO && (!ISOLabelText || !ISOValueText)){
            Debug.LogError("VirtualSnapshotScript:: ISO Label and/or Value UI Text is missing");
        }
        if(showExposures && (!ExposuresLabelText || !ExposuresRemainingText)){
            Debug.LogError("VirtualSnapshotScript:: Exposures Label and/or Exposures Remaining UI Text is missing");
        }
        if(!shutter || !mainCameraCanvas || !cameraTransitionImage){
            Debug.LogError("VirtualSnapshotScript:: Missing one or more Camera UI components. You may have broken the prefab.");
        }

        // Depending on your target platform, you may want to use Application.persistentDataPath instead
        snapshotDirectory = Application.dataPath + "/snapshots/";

        // Ensure the snapshot directory exists
        try{
            if(!Directory.Exists(snapshotDirectory)){
                Directory.CreateDirectory(snapshotDirectory);
            }
        }catch(IOException e){
            Debug.LogError("VirtualSnapshotScript:: There was a problem creating the snapshots directory! ("+ e.Message +")");
        }

        cameraFlash = GameObject.Find("CameraFlashAndSounds");
        if(!cameraFlash){
            Debug.LogError("VirtualSnapshotScript:: There was a problem finding the Camera Flash");
        }

        thePlayer = GameObject.FindGameObjectWithTag("Player");
        if (!thePlayer){
            Debug.LogError("VirtualSnapshotScript:: No object in scene tagged as 'Player'");
        }

        getStartSensitivity();

        if(limitExposureCount){
            exposuresRemaining = snapshotLimit;
        }

    }

	void Start(){

        // Ensure UI colors are applied, or disabled
        if(showAperture){
            apertureText.GetComponent<Text>().color = CameraUITextColor;
        }else{
            apertureText.GetComponent<Text>().enabled = false;
        }
        if(showShutterSpeed){
            shutterSpeedText.GetComponent<Text>().color = CameraUITextColor;
        }else{
            shutterSpeedText.GetComponent<Text>().enabled = false;
        }
        if(showISO){
            ISOLabelText.GetComponent<Text>().color = CameraUITextColor;
            ISOValueText.GetComponent<Text>().color = CameraUITextColor;
        }else{
            ISOLabelText.GetComponent<Text>().enabled = false;
            ISOValueText.GetComponent<Text>().enabled = false;
        }
        if(showExposures){
            ExposuresLabelText.GetComponent<Text>().color = CameraUITextColor;
            ExposuresRemainingText.GetComponent<Text>().color = CameraUITextColor;
            if(limitExposureCount){
                ExposuresRemainingText.GetComponent<Text>().text = ""+exposuresRemaining;
            }
        }else{
            ExposuresLabelText.GetComponent<Text>().enabled = false;
            ExposuresRemainingText.GetComponent<Text>().enabled = false;
        }
        if(showBatteryIndicator){
            batteryLevelIcon.GetComponent<Image>().color = BatteryIconColorFilled;
            setBatteryLevel(batteryLevel);
        }else{
            batteryLevelIcon.GetComponent<Image>().enabled = false;
        }
        if(showFlashIndicator){
            flashIndicatorIcon.GetComponent<Image>().color = BatteryIconColorFilled;
            if(cameraFlashEnabled){
                flashIndicatorIcon.GetComponent<Image>().overrideSprite = flashIndicatorIconSprites[0];
            }else{
                flashIndicatorIcon.GetComponent<Image>().overrideSprite = flashIndicatorIconSprites[1];
            }
        }else{
            flashIndicatorIcon.GetComponent<Image>().enabled = false;
        }
        if(showLightMeter){
            lightMeter.GetComponent<Image>().color = CameraUITextColor;
        }else{
            lightMeter.GetComponent<Image>().enabled = false;
        }

        // Start with both canvases disabled
        mainCameraCanvas.GetComponent<Canvas>().enabled = false;
        cameraTransitionImage.transform.parent.GetComponent<Canvas>().enabled = false;

	}
	
	// Update is called once per frame
	void Update(){

        // Toggle camera up/down
        if(Input.GetKey(KeyCode.C)){

            if(cameraUp){

                if(!movingCameraDown){

                    movingCameraDown = true;
                    cameraTransitionImage.transform.parent.GetComponent<Canvas>().enabled = true;
                    cameraTransitionImage.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

                    resetMouseLookSensitivity();
                    if(zoomEnabled){
                        previousZoom = currentZoom;
                        targetZoom = startZoom;
                    }

                    cameraFlash.GetComponent<AudioSource>().clip = cameraOff;
                    cameraFlash.GetComponent<AudioSource>().Play();

                }

            }else{

                if(!movingCameraUp){

                    movingCameraUp = true;
                    cameraTransitionImage.transform.parent.GetComponent<Canvas>().enabled = true;
                    cameraTransitionImage.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
                    if(zoomEnabled){
                        targetZoom = previousZoom;
                    }

                    cameraFlash.GetComponent<AudioSource>().clip = cameraOn;
                    cameraFlash.GetComponent<AudioSource>().Play();

                }

            }

        }

        // Handle camera up/down transitions
        if(movingCameraUp){

            cameraTransitionImage.GetComponent<CanvasRenderer>().SetAlpha(cameraTransitionImage.GetComponent<CanvasRenderer>().GetAlpha() + 0.1f);

            if(cameraTransitionImage.GetComponent<CanvasRenderer>().GetAlpha() >= 1.0f){

                cameraUp = true;
                movingCameraUp = false;
                cameraTransitionImage.transform.parent.GetComponent<Canvas>().enabled = false;
                mainCameraCanvas.GetComponent<Canvas>().enabled = true;

            }

        }else if(movingCameraDown){

            cameraTransitionImage.GetComponent<CanvasRenderer>().SetAlpha(cameraTransitionImage.GetComponent<CanvasRenderer>().GetAlpha() + 0.1f);

            if(cameraTransitionImage.GetComponent<CanvasRenderer>().GetAlpha() >= 1.0f){

                cameraUp = false;
                movingCameraDown = false;
                cameraTransitionImage.transform.parent.GetComponent<Canvas>().enabled = false;
                mainCameraCanvas.GetComponent<Canvas>().enabled = false;

            }

        }

        // Only allow zoom, taking pictures, etc. when camera is up (and not otherwise transitioning)
        if(cameraUp && !movingCameraUp && !movingCameraDown){

            if(snapshotCountdown > 0.0f){
                snapshotCountdown -= Time.deltaTime;
            }

            // Only allowed to take a snapshot if not already in progress, and enough time has elapsed since last snapshot, and if we have exposures left
            if((Input.GetKeyDown(KeyCode.T) || Input.GetMouseButtonDown(0)) && !takingSnapshot && (snapshotCountdown <= 0.0f)){

                if(exposuresRemaining > 0){

                    cameraTargetCapturedInLastSnapshot = false;

                    if(limitExposureCount){
                        exposuresRemaining -= 1;
                    }

                    if(cameraFlashEnabled){
                        cameraFlash.GetComponent<Light>().enabled = true;
                    }

                    cameraFlash.GetComponent<AudioSource>().clip = shutterSounds[Random.Range(0,shutterSounds.Length)];
                    cameraFlash.GetComponent<AudioSource>().Play();

                    snapshotCountdown = minimumSnapshotInterval;
                    takingSnapshot = true;
                    shutterTimeRemaining = shutterDurationInSeconds;
                    StartCoroutine(takePhoto());

                }else{

                    cameraFlash.GetComponent<AudioSource>().clip = shutterNoExposuresLeft;
                    cameraFlash.GetComponent<AudioSource>().Play();

                }

            }

            // Toggle Camera Flash On/Off
            if(Input.GetKeyDown(KeyCode.F) && !takingSnapshot && snapshotCountdown <= 0.0f){

                cameraFlashEnabled = !cameraFlashEnabled;

                if(cameraFlashEnabled){
                    flashIndicatorIcon.GetComponent<Image>().overrideSprite = flashIndicatorIconSprites[0];
                    cameraFlash.GetComponent<AudioSource>().clip = flashTurnOn;
                    cameraFlash.GetComponent<AudioSource>().Play();
                }else{
                    flashIndicatorIcon.GetComponent<Image>().overrideSprite = flashIndicatorIconSprites[1];
                    cameraFlash.GetComponent<AudioSource>().clip = flashTurnOff;
                    cameraFlash.GetComponent<AudioSource>().Play();
                }

            }
            
            // Handle Zoom
            if(zoomEnabled){

                // For keys held down
                if(Input.GetKey(KeyCode.Equals)){
                    targetZoom = Mathf.Max(targetZoom - 60.0f*Time.deltaTime, minZoom);
                }
                if(Input.GetKey(KeyCode.Minus)){
                    targetZoom = Mathf.Min(targetZoom + 60.0f*Time.deltaTime, maxZoom);
                }

                // For mouse wheel
                if(Input.mouseScrollDelta.y > 0){
                    targetZoom = Mathf.Max(targetZoom - 500.0f*Time.deltaTime, minZoom);
                }
                if(Input.mouseScrollDelta.y < 0){
                    targetZoom = Mathf.Min(targetZoom + 500.0f*Time.deltaTime, maxZoom);
                }

                currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);

                // If zoom changed, adjust sensitivity
                if(currentZoom != snapshotCamera.fieldOfView){

                    if(!cameraFlash.GetComponent<AudioSource>().isPlaying) {
                        cameraFlash.GetComponent<AudioSource>().clip = cameraZoomLoop;
                        cameraFlash.GetComponent<AudioSource>().Play();
                    }

                    adjustMouseLookSensitivity();

                }else{

                    if(cameraFlash.GetComponent<AudioSource>().isPlaying && cameraFlash.GetComponent<AudioSource>().clip == cameraZoomLoop) {
                        cameraFlash.GetComponent<AudioSource>().Stop();
                    }

                }

                snapshotCamera.fieldOfView = currentZoom;

            }

            // This simulates shutter visuals
            if(takingSnapshot){

                shutterTimeRemaining -= Time.deltaTime;

                if(shutterTimeRemaining <= 0.0f){
                    shutter.GetComponent<Image>().enabled = false;
                    takingSnapshot = false;
                    cameraFlash.GetComponent<Light>().enabled = false;
                }else{
                    shutter.GetComponent<Image>().enabled = true;
                }

            }

        }else{

            // We also adjust here to cover smooth zoom changes during camera up/down transitions
            if(zoomEnabled){
                currentZoom = Mathf.Lerp(currentZoom, targetZoom,0.5f);
                if(currentZoom != snapshotCamera.fieldOfView) {
                    adjustMouseLookSensitivity();
                }
                snapshotCamera.fieldOfView = currentZoom;
            }

        }

	}

    // Depending on your implementation, you may want to adjust sensitivity a different way
    // Alternatively, if you desire accurate simulation, don't adjust sensitivity at all. When 
    // operating a camera at a long focal length, it is hard to move it in small amounts 
    // accurately. An additional alternative would be to add some kind of wobble or sway
    // when zoomed in.
    private void adjustMouseLookSensitivity(){
        Vector2 adjustedSensitivity = Vector2.zero;
        adjustedSensitivity.x = 1.0f + ((currentZoom/maxZoom) * 4.0f);
        adjustedSensitivity.y = 1.0f + ((currentZoom/maxZoom) * 4.0f);
        thePlayer.GetComponent<MouseLookScript>().setMouseSensitivity(adjustedSensitivity.x, adjustedSensitivity.y);
    }

    // Just reset to starting sensitivity, for use when player puts camera down
    private void resetMouseLookSensitivity(){
        thePlayer.GetComponent<MouseLookScript>().setMouseSensitivity(startSensitivity.x, startSensitivity.y);
    }

    // Get look sensitivity when game starts, to save for restoring it later
    private void getStartSensitivity(){
        startSensitivity.x = thePlayer.GetComponent<MouseLookScript>().getMouseSensitivity().x;
        startSensitivity.y = thePlayer.GetComponent<MouseLookScript>().getMouseSensitivity().y;
    }

    // Very simple function to make a unique filename
    // You can also number these more simply if you want to number the files from film roll, etc.
    // Note: this does not include file extension
    private string getNextPhotoFilename(){
        return "snapshot" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    // This is the function that actually captures the photo, and writes it to file
    IEnumerator takePhoto(){

        yield return new WaitForEndOfFrame();

        // Get RenderTexture data from camera
        RenderTexture rt = new RenderTexture(snapshotWidth, snapshotHeight, 24);
        snapshotCamera.targetTexture = rt;
        snapshotCamera.Render();
        RenderTexture.active = rt;

        // Save actual photo to file
        Texture2D screenShot = new Texture2D(snapshotWidth, snapshotHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, snapshotWidth, snapshotHeight), 0, 0);
        snapshotCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        
        // This assumes PNG encoding. Replace this with whichever encoding you want (and update the file extension from .png)
        byte[] bytes = screenShot.EncodeToPNG();
        string snapshotFilename = snapshotDirectory + getNextPhotoFilename() + fileExtension;

        // Saving the photo may not always work (e.g. if on mobile, user removes their SD card)
        try{
            System.IO.File.WriteAllBytes(snapshotFilename, bytes);
        }catch(IOException e){
            Debug.Log("VirtualSnapshotScript:: Failed to save snapshot '" + snapshotFilename + "' (" + e.Message + ")");
        }

        // Target Capture stuff
        if(cameraTarget){
            
            RaycastHit hitTest;
			Vector3 rayCastDirection = cameraTarget.transform.position - snapshotCamera.transform.position;
            cameraTargetCapturedInLastSnapshot = false;
			
            // Do line of sight test first to ensure it's possible to capture target in the snapshot
            if(Physics.Raycast(snapshotCamera.transform.position, rayCastDirection, out hitTest)){

                // If we can see it, check that it's within the viewfinder according to specified percentage
                if(hitTest.collider.gameObject == cameraTarget){

                    cameraTargetScreenPosition = snapshotCamera.WorldToScreenPoint(cameraTarget.transform.position);
                    Vector2 cameraTargetDelta = Vector2.zero;
                    cameraTargetDelta.x = Mathf.Abs(Screen.width/2 - cameraTargetScreenPosition.x);
                    cameraTargetDelta.y = Mathf.Abs(Screen.height/2 - cameraTargetScreenPosition.y);

                    if((cameraTargetDelta.x/Screen.width) <= cameraTargetDistanceTolerance && (cameraTargetDelta.y/Screen.height) <= cameraTargetDistanceTolerance){
                        cameraTargetCapturedInLastSnapshot = true;
                    }

                }

			}

        }

        if(limitExposureCount){
            ExposuresRemainingText.GetComponent<Text>().text = ""+exposuresRemaining;
            // If that was the last exposure, make the UI text red for easier readability
            if(exposuresRemaining == 0){
                ExposuresRemainingText.GetComponent<Text>().color = NoMoreExposuresTextColor;
            }
        }

    }

    // Update Battery level, and icon Sprite/color (if shown)
    public void setBatteryLevel(int newLevel){
        
        batteryLevel = newLevel;

        if(showBatteryIndicator){

            if(batteryLevel >= 3){
                batteryLevelIcon.GetComponent<Image>().color = BatteryIconColorFilled;
            }else if(batteryLevel >= 2){
                batteryLevelIcon.GetComponent<Image>().color = BatteryIconColorLow;
            }else{
                batteryLevelIcon.GetComponent<Image>().color = BatteryIconColorEmpty;
            }

            batteryLevelIcon.GetComponent<Image>().overrideSprite = batteryLevelSprites[batteryLevel-1];

        }

    }

    public int getBatteryLevel(){
        return batteryLevel;
    }

    public bool capturedTargetInLastSnapshot(){
        return cameraTargetCapturedInLastSnapshot;
    }

}
