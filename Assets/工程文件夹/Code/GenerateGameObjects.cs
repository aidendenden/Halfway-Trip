using System;
using UnityEngine;
using System.IO;
using Random = UnityEngine.Random;

public class GenerateGameObjects : MonoBehaviour
{

	public GameObject prefab; // ��ƬԤ����

	string folderPath; // �ļ���·��
	

	private void OnEnable()
	{
		ReadSnapshots();
	}
	private void OnDisable()
	{
		DeleteChildren(transform);
	}

	void ReadSnapshots()
	{
		folderPath = Path.Combine(Application.dataPath, "snapshots");
		// ��ȡ�ļ����е�����ͼƬ�ļ�
		string[] imageFiles = Directory.GetFiles(folderPath, "*.png");

		// ����ͼƬ����������Ϸ����
		foreach (string file in imageFiles)
		{
			// ��ȡ�ļ�������������չ����
			string fileName = Path.GetFileNameWithoutExtension(file);

			Vector3 PR = new Vector3(Random.Range(-8, 8), Random.Range(-4, 4), Random.Range(-5, 5));

			// �����ļ���������Ϸ����
			GameObject obj = Instantiate(prefab, gameObject.transform.position + PR, Quaternion.identity);
			obj.name = fileName; // ������Ϸ���������Ϊ�ļ���
			obj.transform.parent = gameObject.transform;
			// ���Ը�����Ҫ�����ɵ���Ϸ������н�һ����������������λ�á���ת�����ŵ�
			// obj.transform.position = ...
			// obj.transform.rotation = ...
			// obj.transform.localScale = ...
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