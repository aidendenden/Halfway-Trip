using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using 工程文件夹.Code;

public class ForBlock : MonoBehaviour
{
    public bool isCanMove = true;


    MangerManger _Manager;

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        GameEventManager.OnTrigger += CheckIsCanMove;
        rb = gameObject.GetComponent<Rigidbody>();
        _Manager = GameObject.FindGameObjectWithTag("MangerManger").GetComponent<MangerManger>();
        CheckIsCanMove();


    }

    // Update is called once per frame
    void Update()
    {
       
    }



    public  void CheckIsCanMove(string a,Transform b)
    {
        if(a == "End")
        {
            isCanMove = true;
            Unfreeze();
        }
        if(a == "Open")
        {
            isCanMove = false;
            Freeze();
        }
    }




    public void CheckIsCanMove()
    {
        if (_Manager.CameraMode == 0)
        {
            isCanMove = true;
        }
        if (_Manager.CameraMode == 1)
        {
            isCanMove = false;
        }

        if (isCanMove == false)
        {
            Freeze();
        }
        if (isCanMove == true)
        {
            Unfreeze();
        }
    }
    private void Freeze()
    {
        rb.constraints = RigidbodyConstraints.FreezePosition;

    }

    private void Unfreeze()
    {
        rb.constraints = RigidbodyConstraints.None;
    }
}
