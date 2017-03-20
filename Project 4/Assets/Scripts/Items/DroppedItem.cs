using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DroppedItem : NetworkBehaviour {

	public Item item;

	private ParticleSystem effect;


	public void Awake() {
		transform.SetParent(GameObject.FindGameObjectWithTag("Dropped Items").transform);

		if (GetComponent<NetworkIdentity>() == null)
			gameObject.AddComponent<NetworkIdentity>();

		Mesh mesh = GetComponent<MeshFilter>().mesh;

		float inverseScaleFactor = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z) / 1.5f;
		transform.localScale /= inverseScaleFactor;

		Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
		for (int i = 0; i < colliders.Length; i++)
			colliders[i].enabled = false;

		transform.rotation = Quaternion.Euler(Random.Range(20, 70), transform.eulerAngles.y, transform.eulerAngles.z);
		transform.position += Vector3.up * mesh.bounds.extents.y;
		StartCoroutine(StartTrigger(2f));
	}

	public void Update() {
		transform.Rotate(0, 180 * Time.deltaTime, 0, Space.World);
	}

	private IEnumerator StartTrigger(float delay) {
		yield return new WaitForSeconds(delay);
		effect = GameObject.Instantiate(Game.Instance.droppedItemEffect).GetComponent<ParticleSystem>();
		effect.transform.position = transform.position;
		effect.transform.SetParent(GameObject.FindGameObjectWithTag("Dropped Items").transform, true);

		SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
		trigger.isTrigger = true;
		trigger.radius = 1;
	}

	private IEnumerator PickedUp(Player player) {
		player.GiveItem(item.ItemId);
		gameObject.SetActive(false);

		ParticleSystem.MainModule main = effect.main;
		main.loop = false;
		float additionalTime = main.startLifetime.Evaluate(effect.time / main.duration, 1);

		yield return new WaitForSeconds(effect.main.duration - effect.time + additionalTime);
		GameObject.Destroy(effect.gameObject);
		GameObject.Destroy(gameObject);
		yield break;
	}

	public void OnTriggerStay(Collider other) {
		if (other.tag == "Player")
			StartCoroutine(PickedUp(other.GetComponent<Player>()));
	}
}
