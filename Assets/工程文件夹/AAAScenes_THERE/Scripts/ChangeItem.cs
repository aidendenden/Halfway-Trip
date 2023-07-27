using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeItem : MonoBehaviour
{
    
    public int SelectedItem = 0;
    public GameObject[] ItemList = new GameObject[0];
    public bool IsCanChangeItem;

    private void Update()
    {
        ScrollWheelRoll();
    }












    void ScrollWheelRoll()//�������־��жϣ�
    {
        if (IsCanChangeItem)
        {
            int PastItem = SelectedItem;
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (SelectedItem >= ItemList.Length - 1)
                {
                    SelectedItem = 0;
                }
                else
                {
                    SelectedItem++;

                }
            }


            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (SelectedItem <= 0)
                {
                    SelectedItem = ItemList.Length - 1;
                }
                else
                {
                    SelectedItem--;
                }
            }
            if (PastItem != SelectedItem)
            {
                ChangeHandItem();
            }
        }
    }

    void ChangeHandItem()//����SelectedItem�л���Ʒ�Ŀ���
    {
        int i = 0;
        foreach(GameObject Item in ItemList)
        {
            if(i == SelectedItem)
            {
                Item.SetActive(true);
            }
            else
            {
                Item.SetActive(false);
            }
            i++;
        }
    }

    
}
