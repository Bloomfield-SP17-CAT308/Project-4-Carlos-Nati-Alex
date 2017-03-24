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
		for (int i = 0; i < nearby.Count; i++)
			if (nearby[i] as MonoBehaviour == null || !nearby[i].InteractableActive)
				nearby.RemoveAt(i);

		if (Input.GetKeyDown(KeyCode.Space) && nearby.Count > 0)
				nearby[0].Interact(player);
	}

	public void OnTriggerEnter(Collider other) {
		PlayerInteractable p = other.GetComponent<PlayerInteractable>();
		if (p != null && (p as MonoBehaviour).isActiveAndEnabled && !nearby.Contains(p) && p.InteractableActive)
			nearby.Insert(0, p);
			
	}

	public void OnTriggerExit(Collider other) {
		PlayerInteractable p = other.GetComponent<PlayerInteractable>();
		if (p != null)
			nearby.Remove(p);
	}
}
