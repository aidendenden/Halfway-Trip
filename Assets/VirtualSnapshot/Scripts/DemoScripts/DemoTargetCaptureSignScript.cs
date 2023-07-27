using UnityEngine;
using System.Collections;

//
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoTargetCaptureSignScript : MonoBehaviour {

    private float countdownTime = 0.4f;
    private float timeLeft = 0.0f;
    private GameObject virtualSnapshotObject;
    private GameObject yesLight;
    private GameObject noLight;
    public Material yesLightOn;
    public Material yesLightOff;
    public Material noLightOn;
    public Material noLightOff;

	void Start(){

        timeLeft = countdownTime;

        virtualSnapshotObject = GameObject.FindGameObjectWithTag("VirtualSnapshot");
        yesLight = GameObject.Find("YesLight");
        noLight = GameObject.Find("NoLight");

        if(!virtualSnapshotObject || !yesLight || !noLight){
            Debug.LogError("Virtual Snapshot Demo Level Script:: Cannot find Virtual Snapshot game objects for Capture Sign");
        }

	}
	
	// Update is called once per frame
	void Update(){
	
        if(timeLeft > 0.0f){

            timeLeft -= Time.deltaTime;

            if(timeLeft < 0.0f){
                if(virtualSnapshotObject.GetComponent<VirtualSnapshotScript>().capturedTargetInLastSnapshot()){
                    yesLight.GetComponent<Renderer>().material = yesLightOn;
                    noLight.GetComponent<Renderer>().material = noLightOff;
                }else{
                    yesLight.GetComponent<Renderer>().material = yesLightOff;
                    noLight.GetComponent<Renderer>().material = noLightOn;
                }
            }

        }

        // Note: This is not a very robust approach, and is not intended for production use
        if(Input.GetKeyDown(KeyCode.T) || Input.GetMouseButtonDown(0)){
            timeLeft = countdownTime;
        }

	}

}
