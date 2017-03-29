using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CraftingTable : MonoBehaviour, PlayerInteractable {

	private bool interactableActive = true;

	public bool InteractableActive {
		get { return interactableActive; }
		set { interactableActive = value; }
	}

	//Collect
	public void Interact(Player player) {
		if (!player.isLocalPlayer)
			return;

		player.ShowCraftItemPanel();
	}

	
}
