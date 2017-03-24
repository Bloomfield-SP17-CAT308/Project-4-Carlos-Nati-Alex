using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Game : NetworkBehaviour {

	public GameObject inventPanelPrefab;
	public GameObject craftItemPrefab;
	public GameObject droppedItemEffect;

	private Player localPlayer;

	private static Game instance;
	private static Transform screenCanvas;

	public static Game Instance {
		get { return instance; }
	}

	public static Transform ScreenCanvas {
		get { return screenCanvas; }
	}

	public Player LocalPlayer {
		get { return localPlayer; }
		set { localPlayer = value; }
	}

	public void Awake() {
		if (instance != null && instance != this) {
			DestroyImmediate(gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void Start() {
		SetScreenCanvas();
		StandardItems.LoadStandardItems();
		SceneManager.sceneLoaded += SceneChanged;

		//StartCoroutine(Testing());
	}

	/*public void Testt() {
		Debug.Log("...");
	}

	public void TestPoint() {
		Debug.Log(RectTransformUtility.RectangleContainsScreenPoint(rectTransforms[0], Input.mousePosition));
		//Debug.Log(Input.mousePosition + " is in\nthe rect " + rectTransforms[0].rect +  ": " + rectTransforms[0].rect.Contains(Input.mousePosition));
	}

	private IEnumerator Testing() {
		while (true) {
			TestPoint();
			yield return new WaitForSeconds(0.2f);
		}
	}*/

	private void SceneChanged(Scene scene, LoadSceneMode mode) {
		SetScreenCanvas();
	}

	private void SetScreenCanvas() {
		screenCanvas = GameObject.FindGameObjectWithTag("Screen Canvas").transform;
	}
}
