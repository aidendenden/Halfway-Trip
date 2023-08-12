using UnityEngine;

public class SwitchItems : MonoBehaviour
{
    public GameObject[] items; // ������Ʒ
    public int currentItem = 0;//��ǰ��Ʒ����

    
    public MangerManger mangerManger;
    private int currentItemIndex; // ��ǰ��Ʒ����

   

  

    private void Start()
    {
        currentItemIndex = 0;
        ActivateItem(currentItemIndex); // �����ʼ��Ʒ
    }

    private void Update()
    {
       
        

        if (mangerManger.CameraMode == 0 && !Input.GetMouseButton(0)) {
            // ʹ���������л���Ʒ
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel > 0f)
            {
                // ���Ϲ������л�����һ����Ʒ
                currentItemIndex++;
                if (currentItemIndex >= items.Length)
                {
                    currentItemIndex = 0;
                }
                ActivateItem(currentItemIndex);
            }
            else if (scrollWheel < 0f)
            {
                // ���¹������л�����һ����Ʒ
                currentItemIndex--;
                if (currentItemIndex < 0)
                {
                    currentItemIndex = items.Length - 1;
                }
                ActivateItem(currentItemIndex);
            }
        }
    }

        private void ActivateItem(int index)
        {
            // ����ָ����������Ʒ������������Ʒ
            for (int i = 0; i < items.Length; i++)
            {
                items[i].SetActive(i == index);
            }
        }
    }
