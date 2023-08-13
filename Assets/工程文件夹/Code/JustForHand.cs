using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustForHand : MonoBehaviour
{
    public Animator animator;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            animator.SetTrigger("扔");
        }
        if(Input.GetAxis("Mouse ScrollWheel")<0)
        {
            animator.SetTrigger("缩小");
        }
        if (Input.GetAxis("Mouse ScrollWheel")>0)
        {
            animator.SetTrigger("放大");
        }
        if (Input.GetAxis("Mouse ScrollWheel") == 0)
        {
            animator.SetTrigger("无滚轮");
        }
    }
}
