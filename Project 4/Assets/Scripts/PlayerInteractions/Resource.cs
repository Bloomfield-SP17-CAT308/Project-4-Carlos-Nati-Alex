using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Resource : NetworkBehaviour, PlayerInteractable {

	private bool interactableActive = true;

	public bool InteractableActive {
		get { return interactableActive; }
		set { interactableActive = value; }
	}

	//Collect
	public void Interact(Player player) {
		interactableActive = false;
		if (player.isLocalPlayer)
			player.GiveItem(GetComponent<DroppedItem>().itemId);

		//Collect UI/Wait time

		GameObject.Destroy(gameObject);
	}
}
