using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraMovement : NetworkBehaviour {

	public float maxXRotation = 70;
	public float defaultDistance = 12;

	public float minDistance = 4;
	public float maxDistance = 20;

	public float tumbleSensitivity = 100;
	public float dollySensitivity = 100;

	private Transform playerTransform;
	private Player player;

	private float cameraHorizontal, cameraVertical;
	private Vector3 deltaRotation;
	private Transform playerOrientation;
	private CursorMode cursorMode = CursorMode.ScreenUI;
	private float currentDistance;

	public Transform PlayerTransform {
		get { return playerTransform; }
		set {
			playerTransform = value;
			player = playerTransform.GetComponent<Player>();
		}
	}

	public Transform PlayerOrientation {
		get { return playerOrientation; }
	}

	public CursorMode CursorMode {
		get { return cursorMode; }
	}

	public void Start() {
		playerOrientation = transform.FindChild("Player Orientation");
		transform.rotation = Quaternion.Euler(30, transform.eulerAngles.y, transform.eulerAngles.z);

		currentDistance = defaultDistance;
		ToggleCursorMode();
	}

	public void Update() {
		if (!player.isLocalPlayer)
			return;
		deltaRotation = default(Vector3);

		currentDistance -= Input.GetAxis("Mouse ScrollWheel") * dollySensitivity * Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.Mouse2)) {
			currentDistance = defaultDistance;
			transform.rotation = Quaternion.Euler(30, playerTransform.eulerAngles.y, 0);
			UpdatePosition();
			return;
		}

		if (currentDistance < minDistance)
			currentDistance = minDistance;
		else if (currentDistance > maxDistance)
			currentDistance = maxDistance;


		if (cursorMode == CursorMode.WorldMovement) {
			cameraHorizontal = Input.GetAxis("Mouse X");
			cameraVertical = -Input.GetAxis("Mouse Y");
		}

		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) {
			ToggleCursorMode();
			return;
		}

		if (cameraHorizontal != 0)
			deltaRotation.y = cameraHorizontal;
		if (cameraVertical != 0)
			deltaRotation.x = cameraVertical;
		deltaRotation.z = 0;

		if (cursorMode == CursorMode.WorldMovement) {
			transform.Rotate(deltaRotation * tumbleSensitivity * Time.deltaTime);

			//Make sure x-rotation is not too extreme, and that z-rotation is 0. (The Vector3 components)
			if (transform.eulerAngles.x > maxXRotation && transform.eulerAngles.x <= 180)
				transform.rotation = Quaternion.Euler(maxXRotation, transform.eulerAngles.y, 0);
			else if (transform.eulerAngles.x < 360 - maxXRotation && transform.eulerAngles.x > 180)
				transform.rotation = Quaternion.Euler(360 - maxXRotation, transform.eulerAngles.y, 0);
			else
				transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0);

			//Keep the Player Orientation's transform rotated with this camera only in the y-axis.
			//Keep the Player Orientation's transform's rotation at 0 (relative to the world) in the x and z-axes.
			playerOrientation.rotation = Quaternion.Euler(0, playerOrientation.eulerAngles.y, 0);
		}
		

		UpdatePosition();
	}

	/// <summary>
	/// Based off the current x and z Vector3 component rotations, we will position the camera
	/// the fixed distance away from the player, keeping the camera looking at the player.
	/// </summary>
	private void UpdatePosition() {
		Vector3 offset = new Vector3(
			currentDistance * Mathf.Cos((Mathf.PI / 180) * (transform.eulerAngles.x + 180)) * Mathf.Sin((Mathf.PI / 180) * (transform.eulerAngles.y)),
			-currentDistance * Mathf.Sin((Mathf.PI / 180) * (transform.eulerAngles.x + 180)),
			0
		);

		float args = Mathf.Pow(currentDistance * Mathf.Cos((Mathf.PI / 180) * (transform.eulerAngles.x + 180)), 2) - (Mathf.Pow(currentDistance * Mathf.Cos((Mathf.PI / 180) * (transform.eulerAngles.x + 180)) * Mathf.Sin((Mathf.PI / 180) * (transform.eulerAngles.y)), 2));

		offset.z = (transform.eulerAngles.y < 270 && transform.eulerAngles.y > 90) ? Mathf.Sqrt(args) : -Mathf.Sqrt(args);

		transform.position = playerTransform.position + offset;
	}

	private void ToggleCursorMode() {
		if (cursorMode == CursorMode.ScreenUI) {
			cursorMode = CursorMode.WorldMovement;
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			cursorMode = CursorMode.ScreenUI;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		deltaRotation = default(Vector3);
	}
}