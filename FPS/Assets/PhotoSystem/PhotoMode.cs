 using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 using UnityEngine.Serialization;

 public class PhotoMode : MonoBehaviour {

	public AudioClip shotClip;
	public string screenshotFolderName = "Screenshots";
	public GameObject basePanel;
	public GameObject photoModePanel;
	public GameObject photoSavePanel;
	public Image saveBigImage;
	public GameObject saveContent;
	public GameObject galleryPanel;
	public Image galleryBigImage;
	public GameObject galleryContent;
	public GameObject thumbnailPrefab;
	public Camera photoCamera;
	public Camera playerCamera;
	public ChangeItem _changeItem;
	private string currentShot;

	void Start(){
		//检查screenshots文件夹是否存在，如果不存在则创建
		if(!System.IO.Directory.Exists(Application.dataPath + "/" + screenshotFolderName)){
			System.IO.Directory.CreateDirectory (Application.dataPath + "/" + screenshotFolderName);
		}
	}

	//截图设置
	[Range(0.25f, 4)]
	public float shotScale = 1; 

	public static string ShotName() {
		//如果想保存jpg，就这样写
		//return string.Format("{0}/Screenshot_{1}.jpg",
		return string.Format("{0}/Screenshot_{1}.png", 
			Application.temporaryCachePath, 
			System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	public void Shot() {
		GetComponent<AudioSource> ().PlayOneShot (shotClip);

		int resWidth = Mathf.RoundToInt (Screen.width * shotScale);
		int resHeight = Mathf.RoundToInt (Screen.height * shotScale);
		RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
		photoCamera.GetComponent<Camera>().targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
		photoCamera.GetComponent<Camera>().Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		photoCamera.GetComponent<Camera>().targetTexture = null;
		RenderTexture.active = null; 
		Destroy(rt);
		//如果想保存jpg，就这样写
		// byte[] bytes = screenShot.EncodeToJPG();
		byte[] bytes = screenShot.EncodeToPNG();
		string filename = ShotName();

		System.IO.File.WriteAllBytes(filename, bytes);
		Debug.Log(string.Format("(Temporal) Screenshot saved as {0}", filename));
		saveBigImage.sprite = SpriteFromPath (filename);
		currentShot = filename;

		LoadGallery(saveContent);

		photoModePanel.SetActive (false);
		photoSavePanel.SetActive (true);
		Screen.lockCursor = false;
	}

	void LateUpdate() {
		//在播放和照片模式之间切换
		if (Input.GetMouseButtonDown(1) && !galleryPanel.activeSelf&&!photoSavePanel.activeSelf && _changeItem.SelectedItem == 2) {
			if (basePanel.activeSelf) {
				basePanel.SetActive (false);
				photoModePanel.SetActive (true);
				playerCamera.gameObject.SetActive(false);
				photoCamera.gameObject.SetActive(true);
				//GetComponent<FlyCamera> ().enabled = true;
			} else {
				basePanel.SetActive (true);
				photoModePanel.SetActive (false);
				playerCamera.gameObject.SetActive(true);
				photoCamera.gameObject.SetActive(false);
				//GetComponent<FlyCamera> ().enabled = false;
			}
		}

		if (Input.GetMouseButtonDown(1) && !photoModePanel.activeSelf&&!photoSavePanel.activeSelf&&_changeItem.SelectedItem==3) {
			if (!galleryPanel.activeSelf) {
				LoadGallery (galleryContent);
				basePanel.SetActive (false);
				galleryPanel.SetActive (true);
				Screen.lockCursor = false;
			} else {
				basePanel.SetActive (true);
				galleryPanel.SetActive (false);
				Screen.lockCursor = true;
			}
		}

		//如果玩家处于照片模式，可以点鼠标左键截屏
		if (Input.GetMouseButtonDown(0)&&photoModePanel.activeSelf) 
		{
			Shot ();
			//GetComponent<FlyCamera> ().enabled = false;
		}

		if (Input.GetAxis("Mouse ScrollWheel")!=0&& photoModePanel.activeSelf)
		{
			var f = Input.GetAxis("Mouse ScrollWheel");
			if (f > 0)
			{
				if (photoCamera.fieldOfView - f * 10 >= 30 ) {
					photoCamera.fieldOfView -= f * 10;
					Debug.Log(f * 10 + "BIG");
				}
			}

			else
			{
				if (photoCamera.fieldOfView - f * 10 <= 75 )
				{
					photoCamera.fieldOfView -= f * 10;
					Debug.Log(f * 10 + "Samll");
				}
			}
			//GetComponent<FlyCamera> ().enabled = false;
		}
		

		//如果玩家处于保存模式或画廊模式，便只能返回
		if (Input.GetKeyDown(KeyCode.Escape)&&(photoSavePanel.activeSelf || galleryPanel.activeSelf)) {
			photoModePanel.SetActive (false);
			photoSavePanel.SetActive (false);
			galleryPanel.SetActive (false);
			basePanel.SetActive (true);
			Screen.lockCursor = true;
		}
	}

	Sprite SpriteFromPath(string path){
		var bytes = System.IO.File.ReadAllBytes(path);
		Texture2D texture = new Texture2D(2, 2);
		texture.LoadImage(bytes);
		return Sprite.Create (texture, new Rect (0, 0, texture.width, texture.height),Vector2.zero);
	}

	public void SaveShot(){
		//这里将临时文件夹中的屏幕截图放到所需的文件夹中
		System.IO.File.Copy (currentShot, Application.dataPath + "/" + screenshotFolderName + "/" + System.IO.Path.GetFileName (currentShot));
		Debug.Log(string.Format("Screenshot saved as {0}", Application.dataPath + "/" + screenshotFolderName + "/" + System.IO.Path.GetFileName (currentShot)));
		photoModePanel.SetActive (true);
		photoSavePanel.SetActive (false);
		//GetComponent<FlyCamera> ().enabled = true;
		Screen.lockCursor = true;
	}

	void LoadGallery(GameObject layoutParent){
		//这里将删除以前的缩略图并重新加载所有缩略图
		GameObject[] previousShots;
		foreach (Transform child in layoutParent.transform) {
			if (child.name != "Save") {
				Destroy (child.gameObject);
			}
		}

		string[] shots;
		//如果想保存jpg，就这样写
		//shots = System.IO.Directory.GetFiles (Application.dataPath + "/" + screenshotFolderName, "*.jpg");
		shots = System.IO.Directory.GetFiles (Application.dataPath + "/" + screenshotFolderName, "*.png");

		for(int i = shots.Length - 1; i >= 0; i--){
			Debug.Log ("Found: " + shots [i]);
			GameObject inst = Instantiate(thumbnailPrefab) as GameObject;
			inst.transform.parent = layoutParent.transform;
			inst.transform.localScale = layoutParent.transform.localScale;
			inst.GetComponent<Image>().sprite = SpriteFromPath(shots[i]);
			inst.GetComponent<Button> ().onClick.AddListener(() => {ThumbToBig();});
		}
	}

	public void ThumbToBig(){
		if (galleryPanel.activeSelf) {
			galleryBigImage.sprite = EventSystem.current.currentSelectedGameObject.GetComponent<Image> ().sprite;
		} else if (photoSavePanel.activeSelf) {
			saveBigImage.sprite = EventSystem.current.currentSelectedGameObject.GetComponent<Image> ().sprite;
		}
	}
}
