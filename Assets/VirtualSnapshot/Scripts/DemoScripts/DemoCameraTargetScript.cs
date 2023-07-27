using UnityEngine;
using System.Collections;

//
// This script is required for successfully running the test level.
// However, you can exclude it from your project, as it is to demonstrate
// examples only
// Copyright 2016 While Fun Games
// http://whilefun.com
//
public class DemoCameraTargetScript : MonoBehaviour {

    private Vector3 startPosition = Vector3.zero;
    private Vector3 endPosition = Vector3.zero;
	
    void Start(){

        startPosition = transform.position;
        endPosition = transform.position;
        endPosition.x = transform.position.x + 40.0f;

    }

	void Update(){
	
        transform.position = Vector3.Lerp(startPosition, endPosition, Mathf.PingPong(Time.time*0.2f, 0.5f));

	}

}
