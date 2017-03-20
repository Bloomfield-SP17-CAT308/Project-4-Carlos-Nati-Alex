using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Game : NetworkBehaviour {

	public GameObject inventPanelPrefab;
	public GameObject droppedItemEffect;

	private static Game instance;
	private static Transform screenCanvas;

	public static Game Instance {
		get { return instance; }
	}

	public static Transform ScreenCanvas {
		get { return screenCanvas; }
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
	}

	private void SceneChanged(Scene scene, LoadSceneMode mode) {
		SetScreenCanvas();
	}

	private void SetScreenCanvas() {
		screenCanvas = GameObject.FindGameObjectWithTag("Screen Canvas").transform;
	}
}
