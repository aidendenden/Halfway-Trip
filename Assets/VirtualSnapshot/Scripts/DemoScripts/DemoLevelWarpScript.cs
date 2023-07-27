using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

//
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoLevelWarpScript : MonoBehaviour {

    private Vector3 myRotation = Vector3.zero;

    public string levelToWarpTo = "DemoScene";
    

	// Use this for initialization
	void Start () {
	
        myRotation.y = 0.8f;

	}
	
	// Update is called once per frame
	void Update () {
	
        transform.Rotate(myRotation);


	}

    void OnTriggerEnter(Collider other){

        if(other.gameObject.CompareTag("Player")){
            SceneManager.LoadScene(levelToWarpTo);
        }

    }
}
