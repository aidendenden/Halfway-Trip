using UnityEngine;

public class SwitchItems : MonoBehaviour
{
    public GameObject[] items; // 四种物品
    public int currentItem = 0;//当前物品类型

    
    public MangerManger mangerManger;
    private int currentItemIndex; // 当前物品索引

   

  

    private void Start()
    {
        currentItemIndex = 0;
        ActivateItem(currentItemIndex); // 激活初始物品
    }

    private void Update()
    {
       
        

        if (mangerManger.CameraMode == 0 && !Input.GetMouseButton(0)) {
            // 使用鼠标滚轮切换物品
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel > 0f)
            {
                // 向上滚动，切换到下一个物品
                currentItemIndex++;
                if (currentItemIndex >= items.Length)
                {
                    currentItemIndex = 0;
                }
                ActivateItem(currentItemIndex);
            }
            else if (scrollWheel < 0f)
            {
                // 向下滚动，切换到上一个物品
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
            // 激活指定索引的物品，禁用其他物品
            for (int i = 0; i < items.Length; i++)
            {
                items[i].SetActive(i == index);
            }
        }
    }
