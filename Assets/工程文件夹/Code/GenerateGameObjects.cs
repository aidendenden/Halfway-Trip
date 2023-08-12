using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Random = UnityEngine.Random;
using 工程文件夹.Code;

public class GenerateGameObjects : MonoBehaviour
{
	public GameObject XiangCe;//相册预制体;


	public GameObject XiangPian; // 相片预制体
	public GameObject WuPin; // 物体预制体

	public int QubeNumber = 0;

	string folderPath; // 文件夹路径


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
		// 获取文件夹中的所有图片文件
		string[] imageFiles = Directory.GetFiles(folderPath, "*.png");

		int index = 0;
		
		// 根据图片数量生成游戏物体
		foreach (string file in imageFiles)
		{
			// 获取文件名（不包含扩展名）
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

			// 根据文件名生成游戏物体
			GameObject obj = Instantiate(XiangPian, gameObject.transform.position + pr, Quaternion.identity);
			obj.name = fileName; // 设置游戏物体的名称为文件名
			obj.transform.parent = XiangCe.transform;
			// 可以根据需要对生成的游戏物体进行进一步操作，例如设置位置、旋转、缩放等
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
		// 遍历所有子物体
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			Transform child = parent.GetChild(i);
			// 递归删除子物体的子物体
			DeleteChildren(child);
			// 销毁子物体
			Destroy(child.gameObject);
		}


	}
}