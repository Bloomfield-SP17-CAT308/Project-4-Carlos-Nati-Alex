using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Game : NetworkBehaviour {

	public GameObject inventPanelPrefab;
	public GameObject craftItemPanelPrefab;
	public GameObject droppedItemEffect;

	public AudioClip[] audioSFX;

	private Player localPlayer;
	private AudioSource UIaudio;

	private List<float> respawnTimers;
	private List<int> respawnItemIds;
	private List<Vector3> respawnPositions;

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
		set {
			localPlayer = value;
			if (!localPlayer.isServer)
				return;

			respawnTimers = new List<float>();
			respawnItemIds = new List<int>();
			respawnPositions = new List<Vector3>();
		}
	}

	public AudioSource UIAudio {
		get { return UIaudio; }
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
		SetUIAudio();

		StandardItems.LoadStandardItems();
		SceneManager.sceneLoaded += SceneChanged;

	}

	public void PlayUIAudioSFX(int index, bool overrideCurrent = true) {
		if (index >= audioSFX.Length)
			throw new Exception("audioSFX[" + index + "] is undefined in the Game class. There are " + audioSFX.Length + " sound effects registered.");

		if (!overrideCurrent && UIAudio.isPlaying)
			return;

		UIAudio.clip = audioSFX[index];
		UIAudio.Play();
	}

	public void PlayUIAudioSFX(AudioClip clip, bool overrideCurrent = true) {
		if (!overrideCurrent && UIAudio.isPlaying)
			return;

		UIAudio.clip = clip;
		UIAudio.Play();
	}


	public void DestroyOnServer(GameObject gameObject) {
		LocalPlayer.CmdDestroyOnServer(gameObject.GetComponent<NetworkIdentity>().netId);
	}

	/* Important workflow tip for Networking and spawning game objects
	RpcDrop(droppedObject.GetComponent<NetworkIdentity>().netId);
	}

	[ClientRpc]
	private void RpcDrop(NetworkInstanceId netId) {
		GameObject droppedObject = (isClient) ? ClientScene.FindLocalObject(netId) : NetworkServer.FindLocalObject(netId);

		droppedObject.GetComponent<DroppedItem>().enabled = true;
	*/

	private void SceneChanged(Scene scene, LoadSceneMode mode) {
		if (scene.name == "Enter Multiplayer Game")
			GetComponent<NetworkManagerHUD>().enabled = true;

		SetScreenCanvas();
		SetUIAudio();
	}

	private void SetScreenCanvas() {
		screenCanvas = GameObject.FindGameObjectWithTag("Screen Canvas").transform;
	}

	private void SetUIAudio() {
		GameObject gameObject = GameObject.FindGameObjectWithTag("UI Audio");
		if (gameObject !=  null)
			UIaudio = gameObject.GetComponent<AudioSource>();
	}

	public void SetUIAudio(AudioSource UIAudio) {
		this.UIaudio = UIAudio;
	}

	public void AddRespawn(int itemId, float respawnDelay, Vector3 position) {
		respawnTimers.Add(respawnDelay);
		respawnItemIds.Add(itemId);
		respawnPositions.Add(position);
	}
}
