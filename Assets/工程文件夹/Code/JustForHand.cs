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
            animator.SetTrigger("��");
        }
        if(Input.GetAxis("Mouse ScrollWheel")<0)
        {
            animator.SetTrigger("��С");
        }
        if (Input.GetAxis("Mouse ScrollWheel")>0)
        {
            animator.SetTrigger("�Ŵ�");
        }
        if (Input.GetAxis("Mouse ScrollWheel") == 0)
        {
            animator.SetTrigger("�޹���");
        }
    }
}
