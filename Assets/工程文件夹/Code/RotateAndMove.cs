using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAndMove : MonoBehaviour
{

    float objSpeed = 100;

    public int rotationDirection = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
        {
            if (objSpeed < 1000)
            {
                objSpeed += Time.deltaTime * 100;
            }


        }


        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
        {
            if (objSpeed > 0)
            {
                objSpeed -= Time.deltaTime * 100;
            }

        }

        if (rotationDirection==0)
        {

            this.transform.Rotate(Vector3.forward * objSpeed * Time.deltaTime, Space.Self);
        }
        if (rotationDirection == 1)
        {

            this.transform.Rotate(Vector3.left * objSpeed * Time.deltaTime, Space.Self);
        }
    }
}
