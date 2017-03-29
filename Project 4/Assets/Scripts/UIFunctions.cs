using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIFunctions : MonoBehaviour {

	public GameObject controlsPrefab;

	public void DestroyGameObject(GameObject gameObject) {
		Game.Instance.PlayUIAudioSFX(2);
		Destroy(gameObject);
	}

	public void ToggleActiveSelf(GameObject gameObject) {
		Game.Instance.PlayUIAudioSFX(2);
		gameObject.SetActive(!gameObject.activeSelf);
	}

	public void Drag(RectTransform UIElement) {
		UIElement.position = Input.mousePosition;
		//Debug.Log(RectTransformUtility.ScreenPointToLocalPointInRectangle(Game.ScreenCanvas.transform,
			//Input.mousePosition, Game.ScreenCanvas.worldCamera, out pos);
	}

	public void LoadScene(string name) {
		if (SceneManager.GetActiveScene().name == "Enter Multiplayer Game" && name == "Title Screen")
			Game.Instance.GetComponent<NetworkManagerHUD>().enabled = false;
		SceneManager.LoadScene(name);

		Game.Instance.PlayUIAudioSFX(2);
	}

	public void ShowControls() {
		Game.Instance.PlayUIAudioSFX(2);
		GameObject controls = GameObject.Instantiate(controlsPrefab);
		controls.transform.SetParent(Game.ScreenCanvas, false);
	}

	public void ShowCredits() {
		GameObject credits = Game.ScreenCanvas.FindChild("Credits Background").gameObject;
		if (credits == null) {
			Debug.LogWarning("Unable to find the credits to show.");
			return;
		}

		credits.SetActive(true);
		Game.Instance.PlayUIAudioSFX(2);
	}
}
