using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotTrigger : EventTrigger {

	private int index = -1;

	private RectTransform inventPanel;
	private RectTransform craftItemPanel;
	private Vector3 originalPos;
	private Transform origParent;
	private Player player;

	private bool isInInvent;

	public int Index {
		get { return index; }
	}

	public void Start() {
		index = int.Parse(transform.parent.name.Substring(5));

		originalPos = transform.localPosition;
		origParent = transform.parent;

		player = Game.Instance.LocalPlayer;
		inventPanel = player.InventoryPanel.transform as RectTransform;
		craftItemPanel = player.CraftItemPanel.transform as RectTransform;

		isInInvent = origParent.parent.name.Contains("Item Inventory Panel");
	}

	public void BeginItemDrag() {
		transform.SetParent(Game.ScreenCanvas);
	}

	public void DragItemWithMouse() {
		transform.position = Input.mousePosition;
	}

	public void EndItemDrag() {
		Vector3 endPos = Input.mousePosition;

		transform.SetParent(origParent);
		transform.localPosition = originalPos;

		//Bad Note: Copy and pasted here..
		if (inventPanel.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(inventPanel, endPos)) {
			int childCount = inventPanel.childCount;
			RectTransform child;
			for (int i = 0; i < childCount; i++) {
				child = inventPanel.GetChild(i) as RectTransform;
				if (!child.name.StartsWith("Slot"))
					continue;
				if (RectTransformUtility.RectangleContainsScreenPoint(child, endPos)) {
					if (isInInvent)
						player.SwapItems(index, child.FindChild("Item").GetComponent<ItemSlotTrigger>().index);
					else
						player.SwapWithCraftItem(child.FindChild("Item").GetComponent<ItemSlotTrigger>().index, index);
					return;
				}
			}
		} else if (craftItemPanel.gameObject.activeSelf && RectTransformUtility.RectangleContainsScreenPoint(craftItemPanel, endPos)) {
			int childCount = craftItemPanel.childCount;
			RectTransform child;
			for (int i = 0; i < childCount; i++) {
				child = craftItemPanel.GetChild(i) as RectTransform;
				if (!child.name.StartsWith("Slot"))
					continue;

				if (RectTransformUtility.RectangleContainsScreenPoint(child, endPos)) {
					if (isInInvent)
						player.SwapWithCraftItem(index, child.FindChild("Item").GetComponent<ItemSlotTrigger>().index);
					else
						player.SwapTwoCraftItems(child.FindChild("Item").GetComponent<ItemSlotTrigger>().index, index);
					return;
				}
			}
		} else {
			player.DropItem(index);
		}
	}
}
