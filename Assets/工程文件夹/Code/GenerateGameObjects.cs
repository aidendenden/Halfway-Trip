using System;
using UnityEngine;
using System.IO;
using Random = UnityEngine.Random;

public class GenerateGameObjects : MonoBehaviour
{

	public GameObject prefab; // 相片预制体

	string folderPath; // 文件夹路径
	

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
		// 获取文件夹中的所有图片文件
		string[] imageFiles = Directory.GetFiles(folderPath, "*.png");

		// 根据图片数量生成游戏物体
		foreach (string file in imageFiles)
		{
			// 获取文件名（不包含扩展名）
			string fileName = Path.GetFileNameWithoutExtension(file);

			Vector3 PR = new Vector3(Random.Range(-8, 8), Random.Range(-4, 4), Random.Range(-5, 5));

			// 根据文件名生成游戏物体
			GameObject obj = Instantiate(prefab, gameObject.transform.position + PR, Quaternion.identity);
			obj.name = fileName; // 设置游戏物体的名称为文件名
			obj.transform.parent = gameObject.transform;
			// 可以根据需要对生成的游戏物体进行进一步操作，例如设置位置、旋转、缩放等
			// obj.transform.position = ...
			// obj.transform.rotation = ...
			// obj.transform.localScale = ...
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