using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

	private bool inCombat = false;

	public float gravityMultiplier = 1f;
	public float initialJumpSpeed = 10f;
	public float speed = 10f;

	public Camera cameraPrefab;
	private Camera camera;
	private Transform rightHand;

	private CharacterController controller;
	private CameraMovement cameraMovement;

	private float vSpeed = 0;
	private Vector3 displacement;

	private float horizontal, vertical;

	private GameObject inventPanel;
	private Item[] inventItems = new Item[9];
	private int[] inventQuantities = new int[9];
	Image[] inventSlotImages;
	private int availableInventSlots = 9;

	public float Height {
		get { return controller.height; }
	}

	public void Awake() {
		controller = GetComponent<CharacterController>(); //Needs to be in Awake for the Height property to be available early
	}

	public override void OnStartLocalPlayer() {
		camera = GameObject.Instantiate(cameraPrefab);
		cameraMovement = camera.GetComponent<CameraMovement>();
		cameraMovement.PlayerTransform = transform;

		GetComponentInChildren<MeshRenderer>().material.color = Color.white;
		transform.position = GameObject.Find("Player Start").transform.position;
		rightHand = transform.FindChild("Positions").FindChild("Right Hand");

		inventPanel = GameObject.Instantiate(Game.Instance.inventPanelPrefab);
		inventPanel.transform.SetParent(Game.ScreenCanvas, false);
		inventPanel.SetActive(false);

		inventSlotImages = new Image[9];
		for (int i = 0; i < inventSlotImages.Length; i++)
			inventSlotImages[i] = inventPanel.transform.FindChild("Slot " + i).GetChild(0).GetComponent<Image>();

	}

	public void Update() {
		if (!isLocalPlayer)
			return;

		if (Input.GetKeyDown(KeyCode.I))
			inventPanel.SetActive(!inventPanel.activeSelf);

		if (Input.GetKeyDown(KeyCode.G))
			Drop(StandardItems.Items[UnityEngine.Random.Range(0, StandardItems.Items.Count)]);
	}

	public void FixedUpdate() {
		if (!isLocalPlayer)
			return;
		
		//Movement-Related Logic -- Seems to work smoother in FixedUpdate.
		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");

		displacement = speed * Vector3.Normalize(new Vector3(horizontal, 0, vertical));

		vSpeed -= 9.81f * gravityMultiplier * Time.fixedDeltaTime;
		if (controller.isGrounded) {
			vSpeed = 0;
			if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Space))
				vSpeed = initialJumpSpeed;
		}
		//displacement.y = vSpeed; //This would be bad because we are transforming the direction, so displacement.y becomes unaligned with the world's y-axis

		//This also restricts the player's rotation -- we no longer rotate in the x or z-axes because of the playerOrientation transform!
		if (displacement.magnitude > 0.3f)
			transform.forward = cameraMovement.PlayerOrientation.TransformDirection(displacement).normalized;


		controller.Move(cameraMovement.PlayerOrientation.TransformDirection(displacement * Time.fixedDeltaTime) + (Vector3.up * vSpeed * Time.fixedDeltaTime));
	}

	private IEnumerator EndCombat(float delay) {
		yield return new WaitForSeconds(delay);
		inCombat = false;
	}




	//Item Methods Begin Here
	public void GiveItem(int itemId, int quantity = 1, bool givePartial = true) {
		Item item = StandardItems.Items[itemId];
		int spaceRequired;
		if (quantity == 1)
			spaceRequired = 1;
		else {
			spaceRequired = quantity / item.StackLimit;
			if (quantity % item.StackLimit != 0)
				spaceRequired++;
		}

		if (availableInventSlots == 0)
			return;
		else if (!givePartial && spaceRequired > availableInventSlots)
			return;

		int remaining = quantity;
		for (int i = 0; i < inventItems.Length; i++) {
			if (inventItems[i] == null) {
				inventItems[i] = item;
				inventQuantities[i] = (quantity < item.StackLimit) ? quantity : item.StackLimit;
				inventSlotImages[i].sprite = item.Sprite;
				inventSlotImages[i].color = Color.white;
				availableInventSlots--;
				remaining--;
				Debug.Log("Putting " + item.Name + " (Quantity: " + inventQuantities[i] + ") at Inventory Slot " + i + ".");
			}
			if (availableInventSlots == 0 || remaining == 0)
				return;
		}
	}

	public Item RemoveItem(int inventSlotIndex) {
		Item item = inventItems[inventSlotIndex];
		inventQuantities[inventSlotIndex] = 0;
		availableInventSlots++;
		return item;
	}

	public Item RemoveFirstItem(int itemId) {
		for (int i = 0; i < inventItems.Length; i++) {
			if (inventItems[i].ItemId == itemId)
				return RemoveItem(i);
		}
		return null;
	}

	public void DropItem(int inventSlotIndex) {
		Item item = RemoveItem(inventSlotIndex);
		Drop(item);
	}

	public void DropFirstItem(int itemId) {
		Item item = RemoveFirstItem(itemId);
		if (item == null)
			return;
		Drop(item);
	}

	private void Drop(Item item) {
		GameObject droppedObject = GameObject.Instantiate(item.Prefab);

		RaycastHit hit;
		if (Physics.Raycast(new Ray(transform.position + 2 * transform.forward + 2 * Vector3.up, Vector3.down), out hit, 20, 1 << 8))
			droppedObject.transform.position = hit.point;
		else
			droppedObject.transform.position = transform.position;

		DroppedItem droppedItem = droppedObject.AddComponent<DroppedItem>();
		droppedItem.item = item;

		NetworkServer.Spawn(droppedObject);
	}
}