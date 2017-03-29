using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtraBackgroundBehaviour : MonoBehaviour {

	public Color a;
	public Color b;

	private Image background;

	public void Start() {
		background = GetComponent<Image>();
	}

	public void Update() {
		background.color = SpecialInterpolation();
	}

	private Color SpecialInterpolation() {
		return b + (a - b) * (Mathf.Exp(Mathf.Cos(Time.time))/ Mathf.Exp(1));
	}
}
