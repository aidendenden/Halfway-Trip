using UnityEngine;
using System.Collections;
using System.IO;

public class CameraSwitch : MonoBehaviour
{
	public GameObject XiangCe;//相册父物体；

	public GameObject[] cameras;
	public string[] shotcuts;
	public bool changeAudioListener = true;


    private void Start()
    {
		//固定光标
		Cursor.lockState = CursorLockMode.Locked;
		// 隐藏光标
		Cursor.visible = false;
	}


    void Update()
	{
		int i = 0;
		for (i = 0; i < cameras.Length; i++)
		{
			if (Input.GetKeyUp(shotcuts[i]))
				SwitchCamera(i);
		}
	}

	void SwitchCamera(int index)
	{
		int i = 0;
		for (i = 0; i < cameras.Length; i++)
		{
			if (i != index)
			{
				if (changeAudioListener)
				{
					cameras[i].GetComponent<AudioListener>().enabled = false;
				}
				cameras[i].GetComponent<Camera>().enabled = false;


				
			}
			else
			{
				if (changeAudioListener)
				{
					cameras[i].GetComponent<AudioListener>().enabled = true;
				}
				cameras[i].GetComponent<Camera>().enabled = true;
				if (i == 1)
				{
					ChangeToXiangCe();

				}
                else
                {
					ChangeToChangJing();


				}



			}
		}
	}


	void ChangeToXiangCe()
    {
		XiangCe.SetActive(true);
		// 光标自由
		Cursor.lockState = CursorLockMode.Confined;
		// 显示光标
		Cursor.visible = true;
	}

	void ChangeToChangJing()
	{
		XiangCe.SetActive(false);
		// 将光标锁定到屏幕中间
		Cursor.lockState = CursorLockMode.Locked;
		// 隐藏光标
		Cursor.visible = false;
	}

}
