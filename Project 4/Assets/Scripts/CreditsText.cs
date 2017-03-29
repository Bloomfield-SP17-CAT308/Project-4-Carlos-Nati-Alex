using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsText : MonoBehaviour {

	public int rollSpeed = 150;

	private RectTransform rectTransform;
	private RectTransform textClipping;

	private Image textClippingImg;
	private Image background;
	private Text title;

	public void Awake() {
		rectTransform = transform as RectTransform;
		textClipping = transform.parent.FindChild("Text Clipping").GetComponent<RectTransform>();

		background = transform.parent.GetComponent<Image>();
		title = transform.parent.FindChild("Title").GetComponent<Text>();
		textClippingImg = textClipping.GetComponent<Image>();
	}

	public void OnEnable() {
		StartCoroutine(Fade(false, 2));
	}

	private IEnumerator RollCredits() {
		textClippingImg.color = SetAlpha(textClippingImg.color, 1);
		background.color = SetAlpha(background.color, 1);
		title.color = SetAlpha(title.color, 1);

		Vector2 originalPos = rectTransform.anchoredPosition;
		while (rectTransform.anchoredPosition.y < 0 + Screen.height - textClipping.rect.height) {
			rectTransform.anchoredPosition += rollSpeed * new Vector2(0, Time.deltaTime);

			yield return new WaitForSeconds(0.05f);
		}
		rectTransform.anchoredPosition = originalPos;

		yield return new WaitForSeconds(1);
		StartCoroutine(Fade(true, 2));
		yield break;
	}

	private IEnumerator Fade(bool fadeOut, float duration = 2) {
		if (fadeOut)
			textClippingImg.color = SetAlpha(textClippingImg.color, 0);

		Color origBackground = background.color;
		Color origTitle = title.color;
		float startTime = Time.time;

		while (Time.time < startTime + duration) {
			if (fadeOut) {
				background.color = SetAlpha(origBackground, 1 + (0 - 1) * (Time.time - startTime) / duration);
				title.color = SetAlpha(origTitle, 1 + (0 - 1) * (Time.time - startTime) / duration);
			} else {
				background.color = SetAlpha(origBackground, (Time.time - startTime) / duration);
				title.color = SetAlpha(origTitle, (Time.time - startTime) / duration);
			}

			yield return new WaitForSeconds(0.05f);
		}

		if (fadeOut) {
			background.color = SetAlpha(origBackground, 0);
			title.color = SetAlpha(origTitle, 0);
			transform.parent.gameObject.SetActive(false);
		} else {
			background.color = SetAlpha(origBackground, 1);
			title.color = SetAlpha(origTitle, 1);
			StartCoroutine(RollCredits());
		}
		yield break;
	}

	private Color SetAlpha(Color c, float a) {
		return new Color(c.r, c.g, c.b, a);
	}
}
