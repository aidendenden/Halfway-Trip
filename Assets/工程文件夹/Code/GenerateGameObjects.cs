using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Random = UnityEngine.Random;
using �����ļ���.Code;

public class GenerateGameObjects : MonoBehaviour
{
	public GameObject XiangCe;//���Ԥ����;


	public GameObject XiangPian; // ��ƬԤ����
	public GameObject WuPin; // ����Ԥ����

	public int QubeNumber = 0;

	string folderPath; // �ļ���·��


	public List<Vector3> photoPosition=new List<Vector3>();
	public List<Vector3> itemPosition = new List<Vector3>();

	private void OnEnable()
	{
         ReadSnapshots(); 
		 SpawnCube(); 
		
		GameEventManager.Instance.Triggered("Open", transform);
	}
	private void OnDisable()
	{
		DeleteChildren(transform);
		GameEventManager.Instance.Triggered("End", transform);
	}

	void ReadSnapshots()
	{
		folderPath = Path.Combine(Application.dataPath, "snapshots");
		// ��ȡ�ļ����е�����ͼƬ�ļ�
		string[] imageFiles = Directory.GetFiles(folderPath, "*.png");

		int index = 0;
		
		// ����ͼƬ����������Ϸ����
		foreach (string file in imageFiles)
		{
			// ��ȡ�ļ�������������չ����
			string fileName = Path.GetFileNameWithoutExtension(file);

			Vector3 pr=new Vector3();
			
			if (photoPosition.Count-1<index)
			{
				pr = new Vector3(Random.Range(-8, 8), Random.Range(-4, 4), Random.Range(-5, 5));
			}
			else
			{
				pr = photoPosition[index];
			}

			// �����ļ���������Ϸ����
			GameObject obj = Instantiate(XiangPian, gameObject.transform.position + pr, Quaternion.identity);
			obj.name = fileName; // ������Ϸ���������Ϊ�ļ���
			obj.transform.parent = XiangCe.transform;
			// ���Ը�����Ҫ�����ɵ���Ϸ������н�һ����������������λ�á���ת�����ŵ�
			// obj.transform.position = ...
			// obj.transform.rotation = ...
			// obj.transform.localScale = ...

			index++;
		}
	}

	void SpawnCube()
    {
		for (int i =0;i<QubeNumber;i++) {

			Vector3 pr = new Vector3();
			if (itemPosition.Count - 1 < i)
			{
				pr = new Vector3(Random.Range(-8, 8), Random.Range(-4, 4), Random.Range(-5, 5));
			}
			else
			{
				pr = itemPosition[i];
			}
			
			GameObject obj = Instantiate(WuPin, gameObject.transform.position + pr, Quaternion.identity);
			obj.transform.parent = XiangCe.transform;


			


		}
		


	}



	void DeleteChildren(Transform parent)
	{
		// ��������������
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			Transform child = parent.GetChild(i);
			// �ݹ�ɾ���������������
			DeleteChildren(child);
			// ����������
			Destroy(child.gameObject);
		}


	}
}