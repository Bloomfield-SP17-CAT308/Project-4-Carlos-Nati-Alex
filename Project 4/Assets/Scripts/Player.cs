using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : NetworkBehaviour {

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

	//This is bad. I would try to use an object instead of a billion parallel arrays
	private GameObject inventPanel;
	private Item[] inventItems = new Item[9];
	Image[] inventSlotImages = new Image[9];
	private int availableInventSlots = 9;

	private GameObject craftItemPanel;
	private Item[] craftItems = new Item[2];
	Image[] craftItemImages = new Image[2];

	private List<Item> potentialItemsToCraft;
	private int craftIndex = 0;

	private Image craftResultImage;
	private GameObject craftButton;
	private GameObject craftOtherButton;

	private Stats stats;

	public float Height {
		get { return controller.height; }
	}

	public GameObject InventoryPanel {
		get { return inventPanel; }
	}

	public GameObject CraftItemPanel {
		get { return craftItemPanel; }
	}

	public int InventorySlots {
		get { return inventItems.Length; }
	}

	public Item[] CraftingItems {
		get { return craftItems; }
	}

	public float Exp {
		get { return stats.CurrentLevelExp; }
		set { stats.CurrentLevelExp = value; }
	}

	public void Awake() {
		controller = GetComponent<CharacterController>(); //Needs to be in Awake for the Height property to be available early
	}

	public override void OnStartLocalPlayer() {
		Game.Instance.LocalPlayer = this;

		camera = GameObject.Instantiate(cameraPrefab);
		Game.Instance.SetUIAudio(camera.transform.FindChild("UI Audio").GetComponent<AudioSource>());

		cameraMovement = camera.GetComponent<CameraMovement>();
		cameraMovement.PlayerTransform = transform;

		GetComponentInChildren<MeshRenderer>().material.color = Color.white;
		transform.position = GameObject.Find("Player Start").transform.position;
		rightHand = transform.FindChild("Positions").FindChild("Right Hand");

		inventPanel = GameObject.Instantiate(Game.Instance.inventPanelPrefab);
		inventPanel.transform.SetParent(Game.ScreenCanvas, false);
		inventPanel.SetActive(false);

		craftItemPanel = GameObject.Instantiate(Game.Instance.craftItemPanelPrefab);
		craftItemPanel.transform.SetParent(Game.ScreenCanvas, false);
		craftItemPanel.SetActive(false);

		for (int i = 0; i < inventSlotImages.Length; i++)
			inventSlotImages[i] = inventPanel.transform.FindChild("Slot " + i).GetChild(0).GetComponent<Image>();

		for (int i = 0; i < craftItemImages.Length; i++)
			craftItemImages[i] = craftItemPanel.transform.FindChild("Slot " + i).GetChild(0).GetComponent<Image>();

		craftResultImage = craftItemPanel.transform.FindChild("Result Slot").GetChild(0).GetComponent<Image>();
		craftButton = craftItemPanel.transform.FindChild("Craft Button").gameObject;
		craftButton.GetComponent<Button>().onClick.AddListener(StartCrafting);

		craftOtherButton = craftItemPanel.transform.FindChild("Craft Other Button").gameObject;
		craftOtherButton.GetComponent<Button>().onClick.AddListener(SwitchCraftIndex);

		potentialItemsToCraft = new List<Item>();
		stats = GetComponent<Stats>();
	}

	public void Update() {
		if (!isLocalPlayer)
			return;

		if (Input.GetKeyDown(KeyCode.I))
			inventPanel.SetActive(!inventPanel.activeSelf);

		/*if (Input.GetKeyDown(KeyCode.G))
			CmdDrop(UnityEngine.Random.Range(0, StandardItems.Items.Count));*/
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
			/*if (Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Space))
				vSpeed = initialJumpSpeed;*/
		}
		//displacement.y = vSpeed; //This would be bad because we are transforming the direction, so displacement.y becomes unaligned with the world's y-axis

		//This also restricts the player's rotation -- we no longer rotate in the x or z-axes because of the playerOrientation transform!
		if (displacement.magnitude > 0.3f)
			transform.forward = cameraMovement.PlayerOrientation.TransformDirection(displacement).normalized;


		controller.Move(cameraMovement.PlayerOrientation.TransformDirection(displacement * Time.fixedDeltaTime) + (Vector3.up * vSpeed * Time.fixedDeltaTime));
	}

	[Command]
	public void CmdDestroyOnServer(NetworkInstanceId netId) {
		GameObject.Destroy(NetworkServer.FindLocalObject(netId));
	}


	//Item Methods Begin Here

	public bool GiveItem(int itemId) {
		if (availableInventSlots == 0)
			return false;

		for (int i = 0; i < inventItems.Length; i++) {
			if (inventItems[i] == null) {
				inventItems[i] = StandardItems.Items[itemId];
				inventSlotImages[i].sprite = StandardItems.Items[itemId].Sprite;
				inventSlotImages[i].color = Color.white;
				availableInventSlots--;
				return true;
			}
		}
		return false;
	}

	public Item RemoveItem(int index) {
		Item item = inventItems[index];
		Color previous = inventSlotImages[index].color;

		inventItems[index] = null;
		inventSlotImages[index].color = new Color(previous.r, previous.g, previous.b, 0);
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

	public void SwapItems(int index1, int index2) {
		Item tempItem = inventItems[index1];
		Color tempColor = inventSlotImages[index1].color;
		Sprite tempSprite = inventSlotImages[index1].sprite;

		inventItems[index1] = inventItems[index2];
		inventSlotImages[index1].color = inventSlotImages[index2].color;
		inventSlotImages[index1].sprite = inventSlotImages[index2].sprite;

		inventItems[index2] = tempItem;
		inventSlotImages[index2].color = tempColor;
		inventSlotImages[index2].sprite = tempSprite;
	}

	//Bad Note: Copy and pasted..
	//No support for stacks of same item in one slot
	public void SwapWithCraftItem(int inventIndex, int craftItemIndex) {
		if (craftItems[craftItemIndex] == null && inventItems[inventIndex] != null)
			availableInventSlots++;
		else if (inventItems[inventIndex] == null && craftItems[craftItemIndex] != null)
			availableInventSlots--;

		Item tempItem = craftItems[craftItemIndex];
		Color tempColor = craftItemImages[craftItemIndex].color;
		Sprite tempSprite = craftItemImages[craftItemIndex].sprite;

		craftItems[craftItemIndex] = inventItems[inventIndex];
		craftItemImages[craftItemIndex].color = inventSlotImages[inventIndex].color;
		craftItemImages[craftItemIndex].sprite = inventSlotImages[inventIndex].sprite;

		inventItems[inventIndex] = tempItem;
		inventSlotImages[inventIndex].color = tempColor;
		inventSlotImages[inventIndex].sprite = tempSprite;
		TryCraft();
	}

	public void SwapTwoCraftItems(int index1, int index2) {
		Item tempItem = craftItems[index1];
		Color tempColor = craftItemImages[index1].color;
		Sprite tempSprite = craftItemImages[index1].sprite;

		craftItems[index1] = craftItems[index2];
		craftItemImages[index1].color = craftItemImages[index2].color;
		craftItemImages[index1].sprite = craftItemImages[index2].sprite;

		craftItems[index2] = tempItem;
		craftItemImages[index2].color = tempColor;
		craftItemImages[index2].sprite = tempSprite;
		TryCraft();
	}

	public void DropItem(int index) {
		if (inventItems[index] == null)
			return;
		Item item = RemoveItem(index);
		CmdDrop(item.ItemId);
	}

	public void DropFirstItem(int itemId) {
		Item item = RemoveFirstItem(itemId);
		if (item == null)
			return;
		CmdDrop(itemId);
	}

	[Command]
	private void CmdDrop(int itemId) {
		GameObject droppedObject = GameObject.Instantiate(StandardItems.Items[itemId].Prefab);

		RaycastHit hit;
		if (Physics.Raycast(new Ray(transform.position + 2 * transform.forward + 2 * Vector3.up, Vector3.down), out hit, 20, 1 << 8))
			droppedObject.transform.position = hit.point;
		else
			droppedObject.transform.position = transform.position;

		NetworkServer.Spawn(droppedObject);

		RpcDrop(droppedObject.GetComponent<NetworkIdentity>().netId);
	}

	[ClientRpc]
	private void RpcDrop(NetworkInstanceId netId) {
		GameObject droppedObject = (isClient) ? ClientScene.FindLocalObject(netId) : NetworkServer.FindLocalObject(netId);

		droppedObject.GetComponent<DroppedItem>().enabled = true;
	}

	public void ShowCraftItemPanel() {
		craftItemPanel.SetActive(true);
	}

	public void HideCraftItemPanel() {
		craftItemPanel.SetActive(false);
	}

	public void TryCraft() {
		if (craftItems[0] == null || craftItems[1] == null) {
			potentialItemsToCraft.Clear();
			craftResultImage.color = new Color(0, 0, 0, 0);
			craftButton.SetActive(false);
			craftOtherButton.SetActive(false);
			return;
		}

		if (potentialItemsToCraft.Count > 0 && craftButton.activeSelf)
			return;


		//Shows us if we found craftItems[0] or craftItems[1] in a recipe (true or false)
		bool item0, item1;


		for (int i = 0; i < StandardItems.Items.Count; i++) {
			if (!StandardItems.Items[i].Craftable)
				continue;
			item0 = false;
			item1 = false;

			//CraftingRecipe[0] stores the item Ids, CraftingRecipe[1] stores the quantity which will be ignored for this prototype.
			for (int j = 0; j < StandardItems.Items[i].CraftingRecipe[0].Count; j++) {
				if (craftItems[0].ItemId == StandardItems.Items[i].CraftingRecipe[0][j])
					item0 = true;
				else if (craftItems[1].ItemId == StandardItems.Items[i].CraftingRecipe[0][j])
					item1 = true;

			}
			if (item0 && item1)
				potentialItemsToCraft.Add(StandardItems.Items[i]);
		}

		if (potentialItemsToCraft.Count == 0)
			return;

		craftIndex = 0;
		craftResultImage.sprite = potentialItemsToCraft[craftIndex].Sprite;
		craftResultImage.color = Color.white;
		craftButton.SetActive(true);
		if (potentialItemsToCraft.Count > 1)
			craftOtherButton.SetActive(true);
	}

	public void SwitchCraftIndex() {
		if (potentialItemsToCraft.Count <= 1)
			return;

		craftIndex++;
		if (craftIndex >= potentialItemsToCraft.Count)
			craftIndex = 0;

		craftResultImage.sprite = potentialItemsToCraft[craftIndex].Sprite;
		craftResultImage.color = Color.white;
	}

	public void StartCrafting() {
		if (potentialItemsToCraft.Count == 0)
			return;

		StartCoroutine(CraftAndRetrieve());
	}

	private IEnumerator CraftAndRetrieve() {
		Game.Instance.PlayUIAudioSFX(0);

		int craftItem0Id = craftItems[0].ItemId;
		int craftItem1Id = craftItems[1].ItemId;

		while (Game.Instance.UIAudio.isPlaying)
			yield return new WaitForSeconds(0.1f);

		if (craftItems[0] == null || craftItems[1] == null || craftItems[0].ItemId != craftItem0Id || craftItems[1].ItemId != craftItem1Id)
			yield break;

		GiveItem(potentialItemsToCraft[craftIndex].ItemId);
		Exp += 15;
		potentialItemsToCraft.Clear();

		craftItems[0] = craftItems[1] = null;
		craftItemImages[0].color = craftItemImages[1].color = craftResultImage.color = new Color(0, 0, 0, 0);
		craftButton.SetActive(false);
		craftOtherButton.SetActive(false);

		yield break;
	}
}