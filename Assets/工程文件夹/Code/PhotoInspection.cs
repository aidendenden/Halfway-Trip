using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PhotoInspection : MonoBehaviour
{
    public bool examiningObject;
    public float examinationDeadzone = 0.1f;
    public float examineRotationSpeed = 300.0f;
    public GameObject currentHeldObject = null;
    public Transform interactionObjectExamineLocation;
    public SpriteRenderer photo;
    public TextMeshPro textMesh;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && examiningObject)
        {
            Inspection();
        }
    }

    void Inspection()
    {
        // Examination logic: Position, Rotation, etc.
        // float examinationOffsetUp = 0;
        // float examinationOffsetForward = 0;
        // currentHeldObject.transform.position = interactionObjectExamineLocation.transform.position +
        //                                        Vector3.up * examinationOffsetUp +
        //                                        interactionObjectExamineLocation.transform.forward *
        //                                        examinationOffsetForward;
        float rotationInputX = 0.0f;
        float rotationInputY = 0.0f;

        float examinationChangeX = Input.GetAxis("Mouse X");
        float examinationChangeY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(examinationChangeX) > examinationDeadzone)
        {
            rotationInputX = -(examinationChangeX * examineRotationSpeed * Time.deltaTime);
        }

        if (Mathf.Abs(examinationChangeY) > examinationDeadzone)
        {
            rotationInputY = -(examinationChangeY * examineRotationSpeed * Time.deltaTime);
        }

        currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.up,
            rotationInputX, Space.World);
        currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.right,
            rotationInputY, Space.World);
    }
}