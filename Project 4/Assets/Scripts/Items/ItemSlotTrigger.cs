using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotTrigger : EventTrigger {

	private int inventIndex = -1;

	private RectTransform inventPanel;
	private Vector3 originalPos;
	private Transform origParent;
	private Player player;

	public int InventIndex {
		get { return inventIndex; }
	}

	public void Start() {
		inventIndex = int.Parse(transform.parent.name.Substring(5));

		originalPos = transform.localPosition;
		origParent = transform.parent;

		player = Game.Instance.LocalPlayer;
		inventPanel = player.InventoryPanel.transform as RectTransform;
	}

	public void BeginItemDrag() {
		transform.SetParent(transform.parent.parent);
	}

	public void DragItemWithMouse() {
		transform.position = Input.mousePosition;
	}

	public void EndItemDrag() {
		Vector3 endPos = Input.mousePosition;

		transform.SetParent(origParent);
		transform.localPosition = originalPos;

		if (RectTransformUtility.RectangleContainsScreenPoint(inventPanel, endPos)) {
			int childCount = inventPanel.childCount;
			RectTransform child;
			for (int i = 0; i < childCount; i++) {
				child = inventPanel.GetChild(i) as RectTransform;
				if (!child.name.StartsWith("Slot"))
					continue;
				if (RectTransformUtility.RectangleContainsScreenPoint(child, endPos)) {
					player.SwapItems(inventIndex, child.FindChild("Item").GetComponent<ItemSlotTrigger>().inventIndex);
					break;
				}
			}

		} else {
			player.DropItem(inventIndex);
		}
	}
}
