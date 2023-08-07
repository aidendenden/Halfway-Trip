using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffAndOn : MonoBehaviour
{
   public void ActiveOn()
    {
        gameObject.SetActive(true);
        Debug.Log("ON!");
    }
    public void ActiveOff()
    {
        gameObject.SetActive(false);
        Debug.Log("OFF!");
    }
}
