using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

	public float maxXRotation = 70;
	public float distance = 15;
	public float sensitivity = 50;

	private Transform player;

	private float cameraHorizontal, cameraVertical;
	private Vector3 deltaRotation;
	private Transform playerOrientation;
	private CursorMode cursorMode = CursorMode.ScreenUI;

	public Transform PlayerOrientation {
		get { return playerOrientation; }
	}

	public CursorMode CursorMode {
		get { return cursorMode; }
	}

	public void Start() {
		player = GameObject.FindGameObjectWithTag("Player").transform;
		playerOrientation = transform.FindChild("Player Orientation");
		transform.rotation = Quaternion.Euler(30, transform.eulerAngles.y, transform.eulerAngles.z);
	}

	public void Update() {
		if (cursorMode == CursorMode.WorldMovement) {
			cameraHorizontal = Input.GetAxis("Mouse X");
			cameraVertical = - Input.GetAxis("Mouse Y");
		}
		deltaRotation = default(Vector3);

		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
			ToggleCursorMode();

		if (cameraHorizontal != 0)
			deltaRotation.y = cameraHorizontal;
		if (cameraVertical != 0)
			deltaRotation.x = cameraVertical;
		deltaRotation.z = 0;

		transform.Rotate(deltaRotation * sensitivity * Time.deltaTime);

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


		UpdatePosition();
	}

	/// <summary>
	/// Based off the current x and z Vector3 component rotations, we will position the camera
	/// the fixed distance away from the player, keeping the camera looking at the player.
	/// </summary>
	private void UpdatePosition() {
		Vector3 offset = new Vector3(
			distance * Mathf.Cos((Mathf.PI / 180) * (transform.eulerAngles.x + 180)) * Mathf.Sin((Mathf.PI / 180) * (transform.eulerAngles.y)),
			-distance * Mathf.Sin((Mathf.PI / 180) * (transform.eulerAngles.x + 180)),
			0
		);

		float args = Mathf.Pow(distance * Mathf.Cos((Mathf.PI / 180) * (transform.eulerAngles.x + 180)), 2) - (Mathf.Pow(distance * Mathf.Cos((Mathf.PI / 180) * (transform.eulerAngles.x + 180)) * Mathf.Sin((Mathf.PI / 180) * (transform.eulerAngles.y)), 2));

		offset.z = (transform.eulerAngles.y < 270 && transform.eulerAngles.y > 90) ? Mathf.Sqrt(args) : -Mathf.Sqrt(args);

		transform.position = player.position + offset;
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
	}
}