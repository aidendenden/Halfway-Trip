using UnityEngine;
using System.Collections;
using System.IO;

public class CameraSwitch : MonoBehaviour
{
	public GameObject XiangCe;//��Ḹ���壻

	public GameObject[] cameras;
	public string[] shotcuts;
	public bool changeAudioListener = true;

	



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
					XiangCe.SetActive(true);
					
				}
                else
                {
					XiangCe.SetActive(false);
				}



			}
		}
	}

	
}
