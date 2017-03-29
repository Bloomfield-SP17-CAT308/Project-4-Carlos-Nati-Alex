using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Currently assumes that this gameObject has a DroppedItem script attached to it
public class Resource : NetworkBehaviour, PlayerInteractable {

	private bool interactableActive = true;

	public bool InteractableActive {
		get { return interactableActive; }
		set { interactableActive = value; }
	}

	public void Start() {

	}

	//Collect
	public void Interact(Player player) {
		if (player.isLocalPlayer) {
			if (player.GiveItem(GetComponent<DroppedItem>().itemId)) {
				interactableActive = false;
				Game.Instance.PlayUIAudioSFX(1);
			} else
				return;
		}


		//Collect UI/Wait time
		Game.Instance.AddRespawn(GetComponent<DroppedItem>().itemId, 3, transform.position);
		Game.Instance.DestroyOnServer(gameObject);
	}
}
