using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour {

	//ExpLevel[n] signifies the amount of exp required to get from Level n to Level n + 1. The level starts at 1, so index 0 is a sentinel value.
	public static readonly int[] levelExpReqs = new int[] {
		-1, 20, 30, 50, 75, 100, 150
	};

	public static int MaxLevel {
		get { return levelExpReqs.Length; }
	}

	private Text levelText;
	private Image expBar;

	private int craftingLevel = 1;
	private float currentLevelExp = 0;

	private float targetExp = 0;

	public int CraftingLevel {
		get { return craftingLevel; }
		set {
			if (value > MaxLevel || craftingLevel == MaxLevel)
				return;

			if (value > craftingLevel)
				Game.Instance.PlayUIAudioSFX(3);
			currentLevelExp = 0;
			craftingLevel = value;
			levelText.text = "Lvl " + craftingLevel;
		}
	}

	public float CurrentLevelExp {
		get { return currentLevelExp; }
		set {
			targetExp = value;
			StartCoroutine(ChangeExpGradually());
		}
	}

	private float NormalizedExp {
		get { return (craftingLevel >= MaxLevel) ? 1 : currentLevelExp / levelExpReqs[craftingLevel]; }
	}

	public void Start() {
		levelText = Game.ScreenCanvas.FindChild("Level Text").GetComponent<Text>();
		expBar = Game.ScreenCanvas.FindChild("Exp Bar").FindChild("Fill").GetComponent<Image>();
	}

	private IEnumerator ChangeExpGradually(float duration = 3f) {
		float originalTarget = targetExp;
		float originalExp = currentLevelExp;
		float startTime = Time.time;

		while (Time.time <= startTime + duration) {
			if (CraftingLevel < MaxLevel && currentLevelExp >= levelExpReqs[craftingLevel]) {
				targetExp = originalTarget -= levelExpReqs[craftingLevel];
				originalExp = 0;
				startTime = Time.time;
				duration /= 1.4f; //Divide by this magic number because the duration keeps resetting back  -- This slightly lessens the wait time towards the end after many level ups just in case)
				CraftingLevel++;
			}
			currentLevelExp = originalExp + (Time.time - startTime) * (originalTarget - originalExp) / duration;
			expBar.fillAmount = NormalizedExp;

			if (targetExp != originalTarget)
				yield break;
			yield return new WaitForEndOfFrame();
		}
		currentLevelExp = originalTarget;

		yield break;
	}
}
