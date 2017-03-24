using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note that if you expect a MonoBehaviour script to implement an interface, and the
//Scripts gets destroyed with a GameObject from GameObject.Destroy(...),
//YOU MUST cast the PlayerInteractable to a Component Type like MonoBehaviour for
//a null check to work.
public interface PlayerInteractable {
	bool InteractableActive {
		get;
		set;
	}

	//This method should turn InteractableActive to false if this object should be
	//Removed from the active nearby interactables list on Interact.
	void Interact(Player player);
}
