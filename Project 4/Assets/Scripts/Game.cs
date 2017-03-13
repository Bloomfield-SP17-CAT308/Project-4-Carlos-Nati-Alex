using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Game : NetworkBehaviour {

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

	public override void OnStartLocalPlayer() {
		Debug.Log("Started OnLocalPlayer!");
	}
}
