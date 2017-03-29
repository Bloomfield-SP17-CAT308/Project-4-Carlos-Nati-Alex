using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour {

	private Player player;

	private List<PlayerInteractable> nearby;

	public Player Player {
		get { return player; }
	}

	public void Start() {
		player = GetComponentInParent<Player>();
		nearby = new List<PlayerInteractable>();
	}

	public void Update() {
		if (!player.isLocalPlayer)
			return;
		for (int i = 0; i < nearby.Count; i++) {
			if (nearby[i] as MonoBehaviour == null || !nearby[i].InteractableActive) {
				if (nearby[i] is CraftingTable)
					player.HideCraftItemPanel();
				nearby.RemoveAt(i);
			}
		}

		if (Input.GetKeyDown(KeyCode.Space) && nearby.Count > 0)
				nearby[0].Interact(player);
	}

	private bool ValidInteractableObject(PlayerInteractable p) {
		return p != null && (p as MonoBehaviour).isActiveAndEnabled && !nearby.Contains(p) && p.InteractableActive;
	}

	public void OnTriggerEnter(Collider other) {
		if (!player.isLocalPlayer)
			return;
		PlayerInteractable p = other.GetComponent<PlayerInteractable>();
		if (ValidInteractableObject(p))
			nearby.Insert(0, p);
			
	}

	public void OnTriggerExit(Collider other) {
		if (!player.isLocalPlayer)
			return;
		PlayerInteractable p = other.GetComponent<PlayerInteractable>();
		if (p is CraftingTable)
			player.HideCraftItemPanel();
		nearby.Remove(p);
	}
}
