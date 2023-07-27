using UnityEngine;
using System.Collections;

//
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoLevelScript : MonoBehaviour {

    private bool showHelp = true;
    private bool onGUIEnabled = true;

    private GameObject virtualSnapshotObject;

	// Use this for initialization
	void Start(){
	
        virtualSnapshotObject = GameObject.FindGameObjectWithTag("VirtualSnapshot");
        if(!virtualSnapshotObject){
            Debug.LogError("Virtual Snapshot Demo Level Script:: Cannot find Virtual Snapshot Game Object in Scene!");
        }

	}
	
	// Update is called once per frame
	void Update () {
	
        // disables all OnGUI ouput - for screenshots. You can ignore this.
        if(Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.P)){
            Debug.Log("All OnGUI output disable toggled using Q+P");
            onGUIEnabled = !onGUIEnabled;
        }

        if(Input.GetKeyDown(KeyCode.H)){
            showHelp = !showHelp;
        }

        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)){
            virtualSnapshotObject.GetComponent<VirtualSnapshotScript>().setBatteryLevel(1);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)){
            virtualSnapshotObject.GetComponent<VirtualSnapshotScript>().setBatteryLevel(2);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)){
            virtualSnapshotObject.GetComponent<VirtualSnapshotScript>().setBatteryLevel(3);
        }
        if(Input.GetKeyDown(KeyCode.Alpha4)){
            virtualSnapshotObject.GetComponent<VirtualSnapshotScript>().setBatteryLevel(4);
        }
        if(Input.GetKeyDown(KeyCode.Alpha5)){
            virtualSnapshotObject.GetComponent<VirtualSnapshotScript>().setBatteryLevel(5);
        }

	}

    void OnGUI() {

        if(onGUIEnabled) {

            GUI.contentColor = Color.yellow;
            GUI.backgroundColor = Color.black;

            if(showHelp){

                GUI.Box(new Rect(0,0,215,140),"");
                GUI.Label(new Rect(4,0,256,32), "H - Hide Help");
                GUI.Label(new Rect(4,16,256,32), "C - Toggle Camera Up/Down");
                GUI.Label(new Rect(4,32,256,32), "+ / Mouse Wheel Up - Zoom In");
                GUI.Label(new Rect(4,48,256,32), "- / Mouse Wheel Down - Zoom Out");
                GUI.Label(new Rect(4,64,256,32), "T / Left Mouse - Take Snapshot");
                GUI.Label(new Rect(4,80,256,32), "F - Toggle Flash On/Off");
                GUI.Label(new Rect(4,96,256,32), "1 - 5 - Set Battery Level");
                GUI.Label(new Rect(4, 112, 256, 32), " [ / ] - Adjust Mouse Sensitivity");

            }
            else{

                GUI.Box(new Rect(0,0,120,20),"");
                GUI.Label(new Rect(4,0,256,32), "H - Show Help");

            }

        }

    }

}
