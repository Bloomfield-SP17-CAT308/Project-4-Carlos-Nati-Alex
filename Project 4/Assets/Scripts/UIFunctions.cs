using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIFunctions : MonoBehaviour {
	public void DestroyGameObject(GameObject gameObject) {
		Destroy(gameObject);
	}

	public void ToggleActiveSelf(GameObject gameObject) {
		gameObject.SetActive(!gameObject.activeSelf);
	}

	public void Drag(RectTransform UIElement) {
		UIElement.position = Input.mousePosition;
		//Debug.Log(RectTransformUtility.ScreenPointToLocalPointInRectangle(Game.ScreenCanvas.transform,
			//Input.mousePosition, Game.ScreenCanvas.worldCamera, out pos);
	}
}
