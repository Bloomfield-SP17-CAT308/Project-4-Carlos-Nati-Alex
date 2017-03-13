using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Player : MonoBehaviour {

	private bool inCombat = false;

	public float gravityMultiplier = 1f;
	public float initialJumpSpeed = 10f;
	public float speed = 10f;

	public Camera camera;

	private CharacterController controller;
	private CameraMovement cameraMovement;

	private float vSpeed = 0;
	private Vector3 displacement;

	private float horizontal, vertical;

	public float Height {
		get { return controller.height; }
	}

	public void Awake() {
		controller = GetComponent<CharacterController>(); //Needs to be in Awake for the Height property to be available early

	}

	public void Start() {
		cameraMovement = camera.GetComponent<CameraMovement>();
	}

	public void FixedUpdate() {
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

		if (displacement.magnitude > 0.3f)
			transform.forward = cameraMovement.PlayerOrientation.TransformDirection(displacement).normalized;


		controller.Move(cameraMovement.PlayerOrientation.TransformDirection(displacement * Time.fixedDeltaTime) + (Vector3.up * vSpeed * Time.fixedDeltaTime));
	}

	private IEnumerator EndCombat(float delay) {
		yield return new WaitForSeconds(delay);
		inCombat = false;
	}
}