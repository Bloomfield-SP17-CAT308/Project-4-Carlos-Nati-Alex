using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DroppedItem : NetworkBehaviour {

	public int itemId;

	private ParticleSystem effect;
	private bool pickedUp = false;

	public void Start() {
		if (GetComponent<Resource>() != null)
			GetComponent<Resource>().enabled = false;
		
		transform.SetParent(GameObject.FindGameObjectWithTag("Dropped Items").transform);

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
		if (!pickedUp)
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

	//COMMANDS CAN ONLY RUN ON SCRIPTS FROM THE PLAYER'S UNIQUE, 1 PLAYER OBJECT!!
	//OR ON SCRIPTS ON A GAMEOBJECT WITH CLIENT AUTHORITY OVER A NON-PLAYER OBJECT

	private IEnumerator PickedUp(Player player) {
		//Only try, on this client, to give the item to the player if that player is us -- the Local Player.
		if (player.isLocalPlayer) {
			if (player.GiveItem(itemId)) {
				pickedUp = true;
				Game.Instance.PlayUIAudioSFX(1);
			} else {
				yield break;
			}

		}


		//The effect might be null because some Messages like OnTriggerStay are still sent even though this script is not enabled.
		if (effect != null) {
			MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer r in renderers)
				r.enabled = false;

			ParticleSystem.MainModule main = effect.main;
			main.loop = false;
			float additionalTime = main.startLifetime.Evaluate(effect.time / main.duration, 1);

			yield return new WaitForSeconds(effect.main.duration - effect.time + additionalTime);

			GameObject.Destroy(effect.gameObject);
		}

		//We don't need this because this runs on all clients including the host, and if it is destroyed on the server, then it will automatically unspawn.
		//if (isServer)
		//	NetworkServer.UnSpawn(gameObject);
		GameObject.Destroy(gameObject);
		yield break;
	}

	public void OnTriggerStay(Collider other) {
		if (enabled && !pickedUp && other.tag == "Player")
			StartCoroutine(PickedUp(other.gameObject.GetComponent<Player>()));
	}
}
