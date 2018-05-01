using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	[Header ("Movement")]
	/* Frame occuring factors */
	public float gravity  = 20.0f;
	public float friction  = 6f;                // Ground friction

	/* Movement stuff */
	public float moveSpeed = 7.0f;  // Ground move speed
	public float runSpeed = 10f;
	public float speedMultiplier = 1f;
	[HideInInspector]
	public float curSpeed;
	public float moveAcceleration = 14;
	public float runAcceleration = 10;   // Ground accel
	private float curAcceleration;
	public float runDeacceleration = 10;   // Deacceleration that occurs when running on the ground
	public float airAcceleration = 2.0f;  // Air accel
	public float airDeacceleration = 2.0f;    // Deacceleration experienced when opposite strafing
	public float airControl = 0.3f;  // How precise air control is
	public float sideStrafeAcceleration = 50;   // How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
	public float sideStrafeSpeed = 1;    // What the max speed to generate when side strafing
	public float jumpSpeed = 8.0f;  // The speed at which the character's up axis gains when hitting jump
	public float moveScale = 1.0f;
	public bool hideCursor = true;
	[HideInInspector]
	public bool isStatic = false;
	[HideInInspector]
	public bool isActive = true;

	[Header ("Camera")]
	public float mouseSensitivity = 30.0f;
	public Vector3 defaultViewPosition = new Vector3 (0, .6f, 0); // The height at which the camera is bound to
	public Vector3 crouchViewPosition = new Vector3 (0, .4f, 0);
	[HideInInspector]
	public Vector3 viewPosition;
	private Vector3 viewOffset;
	private Vector3 targetOffset;
	private float forwardLean = .2f;
	private float downwardLean = .2f;
	private Transform  playerView;  // Must be a camera;
	private float curForwardLean;
	[HideInInspector]
	public Vector3 camEuler;
	private Vector3 cameraTilt;
	private Vector3 targetCameraTilt;
	[HideInInspector]
	public Vector3 recoilEuler;

	private CharacterController controller;
	private Vector3 playerVelocity = Vector3.zero;
	private Vector3 hardGroundPoint;
	public LayerMask hardGroundLayer;

	// Q3: players can queue the next jump just before he hits the ground
	private bool wishJump = false;
	[HideInInspector]
	public bool canJump = true;
	private bool jumpIssued = false;
	[HideInInspector]
	public bool cameraEnabled = true;
	[HideInInspector]
	public bool canRun = true;

	// TEMP
	//private float targetFov = 90;

	// UI
	public Text velocityText;


	//Contains the command the user wishes upon the character
	class Cmd {
		public float forwardmove;
		public float rightmove;
	}
	private Cmd cmd  ; // Player commands, stores wish commands that the player asks for (Forward, back, jump, etc)



	void Start() {
		controller = GetComponent<CharacterController> ();
		viewPosition = defaultViewPosition;
		curSpeed = moveSpeed;
		curAcceleration = moveAcceleration;
		playerView = transform.GetChild(0).GetComponent<Camera>().transform;
		camEuler.y = transform.eulerAngles.y;
		Application.targetFrameRate = 60;

		/* Hide the cursor */
		if (hideCursor) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}

		if (IsGrounded ()) {
			StartCoroutine (OnPlayerLand ());
		} else {
			StartCoroutine (OnPlayerAirborne ());
		}

		cmd = new Cmd();
	}

	void Update() {
		// If we're active, add player input
		if (isActive) {
			CheckPlayerInput ();
			QueueJump ();

			if (cameraEnabled) {
				CameraRotation ();
			}
		}

		// If we're not static, calculate player movement
		if (!isStatic) {
			if (controller.isGrounded)
				GroundMove ();
			else if (!controller.isGrounded) {
				AirMove ();
			}
		}
		// Apply player velocity
		Move (playerVelocity * Time.deltaTime);

		// Misc
		CursorStateCheck ();
	}

	/*******************************************************************************************************\
|* MOVEMENT
\*******************************************************************************************************/

	/**
	* Sets the movement direction based on player input
	*/
	void SetMovementDir() {
		if (!isStatic && isActive) {
			cmd.forwardmove = Input.GetAxisRaw ("Vertical");
			cmd.rightmove = Input.GetAxisRaw ("Horizontal");
		} else {
			cmd.forwardmove = 0;
			cmd.rightmove = 0;
		}
	}

	/**
     * Queues the next jump just like in Q3
     */
	void QueueJump() {
		if ((Input.GetKeyDown (KeyCode.Space) || Input.GetAxisRaw ("Mouse ScrollWheel") < 0) && !wishJump && canJump) {
			wishJump = true;
			jumpIssued = true;
			StartCoroutine ("JumpQueueReset");
		}

		if (Input.GetKeyUp (KeyCode.Space)) {
			wishJump = false;
			jumpIssued = false;
		}
	}

	// Jump queueing
	IEnumerator JumpQueueReset() {
		yield return new WaitForSeconds (.001f);
		wishJump = false;
	}

	// Called when player is no longer grounded
	IEnumerator OnPlayerAirborne() {
		float heightPeak = transform.position.y;
		if (jumpIssued) {
			canJump = false;
			StartCoroutine (EnableJump ());
		}

		yield return new WaitForSeconds (.02f);

		// While airborne
		while (!IsGrounded()) {
			// Get the highest peak in the jump
			if (transform.position.y > heightPeak) {
				heightPeak = transform.position.y;
			}
			yield return null;
		}

		jumpIssued = false;
		StartCoroutine (OnPlayerLand ());
	}

	// Called when player lands on the ground
	IEnumerator OnPlayerLand() {
		while (IsGrounded()) {
			yield return null;
		}
		StartCoroutine (OnPlayerAirborne ());
	}

	IEnumerator EnableJump() {
		yield return new WaitForSeconds (.1f);
		canJump = true;
	}

	/**
     * Execs when the player is in the air
     */
	void AirMove() {
		Vector3 wishdir;
		float accel;

		float scale = CmdScale();

		SetMovementDir();

		wishdir = new  Vector3(cmd.rightmove, 0f, cmd.forwardmove);
		wishdir = transform.TransformDirection(wishdir);

		float wishspeed = wishdir.magnitude;
		wishspeed *= curSpeed * speedMultiplier;

		wishdir.Normalize();
		wishspeed *= scale;

		//CPM: Aircontrol
		float wishspeed2 = wishspeed;
		if (Vector3.Dot (playerVelocity, wishdir) < -5) {
			accel = airDeacceleration;
		} else {
			//accel = airAcceleration;
			accel = airDeacceleration;
		}
		//If the player is ONLY strafing left or right
		if (cmd.forwardmove == 0 && cmd.rightmove != 0) {
			if (wishspeed > sideStrafeSpeed) {
				//wishspeed = sideStrafeSpeed;
			}
			//accel = sideStrafeAcceleration;
		}

		Accelerate(wishdir, wishspeed, accel);
		if (airControl > 0)
			AirControl(wishdir, wishspeed2);
		// !CPM: Aircontrol

		playerVelocity.y -= gravity * Time.deltaTime;

		//LEGACY MOVEMENT SEE BOTTOM
	}

	/**
     * Air control occurs when the player is in the air, it allows
     * players to move side to side much faster rather than being
     * 'sluggish' when it comes to cornering.
     */
	void AirControl(Vector3 wishdir, float wishspeed) {
		float zspeed;
		float speed;
		float dot;
		float k;

		//Can't control movement if not moving forward or backward
		if (cmd.forwardmove == 0 || wishspeed == 0) {
			return;
		}

		zspeed = playerVelocity.y;
		playerVelocity.y = 0;
		/* Next two lines are equivalent to idTech's VectorNormalize() */
		speed = playerVelocity.magnitude;
		playerVelocity.Normalize();

		dot = Vector3.Dot(playerVelocity, wishdir);
		k = 32;
		k *= airControl * dot * dot * Time.deltaTime;

		// Change direction while slowing down
		if (dot > 0) {
			playerVelocity.x = playerVelocity.x * speed + wishdir.x * k;
			playerVelocity.y = playerVelocity.y * speed + wishdir.y * k;
			playerVelocity.z = playerVelocity.z * speed + wishdir.z * k;

			playerVelocity.Normalize();
		}

		playerVelocity.x *= speed;
		playerVelocity.y = zspeed; // Note this line
		playerVelocity.z *= speed;


	}

	/**
     * Called every frame when the engine detects that the player is on the ground
     */
	void GroundMove() {
		Vector3 wishdir;

		// Do not apply friction if the player is queueing up the next jump
		if (!wishJump) {
			ApplyFriction (1);
		} else {
			ApplyFriction (0);
		}

		SetMovementDir();

		wishdir = new Vector3(cmd.rightmove, 0, cmd.forwardmove);



		wishdir = transform.TransformDirection(wishdir);
		wishdir.Normalize();

		var wishspeed = wishdir.magnitude;
		wishspeed *= curSpeed * speedMultiplier;
		Accelerate (wishdir, wishspeed, curAcceleration);

		Vector3 vel = controller.velocity;
		vel.y = 0;
		playerVelocity.y = 0;

		if (wishJump) {
			playerVelocity.y = jumpSpeed;
			wishJump = false;
		}
	}

	/**
     * Applies friction to the player, called in both the air and on the ground
     */
	void ApplyFriction(float t) {
		Vector3 vec  = playerVelocity; // Equivalent to: VectorCopy();
		float speed ;
		float newspeed ;
		float control ;
		float drop ;

		vec.y = 0.0f;
		speed = vec.magnitude;
		drop = 0.0f;

		/* Only if the player is on the ground then apply friction */
		if (controller.isGrounded) {
			control = speed < runDeacceleration ? runDeacceleration : speed;
			drop = control * friction * Time.deltaTime * t;
		}

		newspeed = speed - drop;
		if (newspeed < 0)
			newspeed = 0;
		if (speed > 0)
			newspeed /= speed;

		playerVelocity.x *= newspeed;
		playerVelocity.y *= newspeed;
		playerVelocity.z *= newspeed;
	}

	/**
     * Calculates wish acceleration based on player's cmd wishes
     */
	void Accelerate(Vector3 wishdir , float wishspeed, float accel) {
		float addspeed;
		float accelspeed;
		float currentspeed;

		currentspeed = Vector3.Dot(playerVelocity, wishdir);
		addspeed = wishspeed - currentspeed;
		if (addspeed <= 0) {
			return;
		}
		accelspeed = accel * Time.deltaTime * wishspeed;
		if (accelspeed > addspeed) {
			accelspeed = addspeed;
		}

		playerVelocity.x += accelspeed * wishdir.x;
		playerVelocity.z += accelspeed * wishdir.z;
	}

	// Rotate player camera
	void CameraRotation() {
		/* Camera rotation stuff, mouse controls this shit */
		camEuler.x -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * 0.02f;
		camEuler.y += Input.GetAxisRaw("Mouse X") * mouseSensitivity * 0.02f;

		//Clamp the X rotation
		if (camEuler.x < -90)
			camEuler.x = -90;
		else if (camEuler.x > 90)
			camEuler.x = 90;

		Vector3 parentEuler = Vector3.zero;
		if (transform.parent != null) {
			parentEuler = transform.parent.eulerAngles;
		}

		// Camera tilt
		float tiltInput = Input.GetAxisRaw ("Horizontal");
		targetCameraTilt.z -= tiltInput * 14 * Time.deltaTime;
		//Clamp tilt vectors
		targetCameraTilt.x = Mathf.Clamp (targetCameraTilt.x, -2, 2);
		targetCameraTilt.y = Mathf.Clamp (targetCameraTilt.y, -2, 2);
		targetCameraTilt.z = Mathf.Clamp (targetCameraTilt.z, -2, 2);
		// Lerp tilt vectors
		cameraTilt = Vector3.Lerp (cameraTilt, targetCameraTilt, 18 * Time.deltaTime);
		if (tiltInput == 0)
		targetCameraTilt = Vector3.Lerp (targetCameraTilt, Vector3.zero, 10 * Time.deltaTime);

		// Apply camera rotation
		this.transform.rotation = Quaternion.Euler(new Vector3 (0, camEuler.y + recoilEuler.x, 0) + parentEuler); // Rotates the collider
		playerView.rotation = Quaternion.Euler(new Vector3 (camEuler.x + recoilEuler.y, camEuler.y + recoilEuler.x, cameraTilt.z) + parentEuler); // Rotates the camera

		// Set the camera's position to the transform
		float leanPercentage = camEuler.x / 90;
		float ySubtraction = downwardLean * leanPercentage;
		Vector3 targetPos = (transform.up * (viewPosition.y - ySubtraction)) + (transform.forward * forwardLean * leanPercentage);

		// Kick
		viewOffset = Vector3.Lerp (viewOffset, targetOffset, 8 * Time.deltaTime);
		targetOffset = Vector3.Lerp (targetOffset, Vector3.zero, 22 * Time.deltaTime);

		// Apply camera transform
		playerView.position = transform.position + targetPos + playerView.transform.TransformVector (viewOffset);

	}

	public void AddCameraOffset (Vector3 offset) {
		targetOffset += offset;
	}

	/*
    ============
    PM_CmdScale
    Returns the scale factor to apply to cmd movements
    This allows the clients to use axial -127 to 127 values for all directions
    without getting a sqrt(2) distortion in speed.
    ============
    */
	float CmdScale() {
		float max = 0;
		float total;
		float scale;

		max = Mathf.Abs(cmd.forwardmove);
		if (Mathf.Abs (cmd.rightmove) > max) {
			max = Mathf.Abs (cmd.rightmove);
		}
		if (max == 0) {
			return 0;
		}

		total = Mathf.Sqrt(cmd.forwardmove * cmd.forwardmove + cmd.rightmove * cmd.rightmove);
		scale = curSpeed * max / (moveScale * total);

		return scale;
	}

	void CheckPlayerInput() {

		// Crouching
		if (Input.GetKeyDown (KeyCode.LeftControl) && isActive && !isStatic) {
			viewPosition = crouchViewPosition;
		}

		if (Input.GetKeyUp (KeyCode.LeftControl) && isActive && !isStatic) {
			viewPosition = defaultViewPosition;
		}

		// Running
		if (Input.GetKeyDown (KeyCode.LeftShift) && isActive && !isStatic && cmd.forwardmove > 0 && canRun) {
			curSpeed = runSpeed;
			curAcceleration = runAcceleration;
			//targetFov = 105;
		}

		if (Input.GetKeyUp (KeyCode.LeftShift)) {
			curSpeed = moveSpeed;
		}

		if (curSpeed == moveSpeed) {
			//targetFov = 90;
			curAcceleration = moveAcceleration;
		}

		if (cmd.forwardmove <= 0) {
			curSpeed = moveSpeed;
		}

		//player.cam.fieldOfView = Mathf.Lerp (player.cam.fieldOfView, targetFov, 6 * Time.deltaTime);
	}

	void CursorStateCheck() {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		/* Ensure that the cursor is locked into the screen */
		if (Cursor.lockState == CursorLockMode.None)
		{
			if (Input.GetMouseButtonDown(0) && !isStatic && isActive)
				Cursor.lockState = CursorLockMode.Locked;
		}
	}

	public bool IsGrounded() {
		Vector3 ray = transform.position + controller.center;
		RaycastHit hit;


		if (Physics.SphereCast (ray, controller.height / 2, -Vector3.up, out hit, .34f, hardGroundLayer, QueryTriggerInteraction.Ignore)) {
			return true;
		} else {
			return false;
		}
	}

	public bool IsHardGrounded() {
		// Raycast if player hits the ground
		Ray ray = new Ray (transform.position, -Vector3.up);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, controller.bounds.size.y / 2 + .25f, hardGroundLayer, QueryTriggerInteraction.Ignore)) {
			// Ray hits the ground so return true
			hardGroundPoint.y = hit.point.y;
			return true;
		} 

		// Player doesn't hit the ground so return false
		else {
			return false;
		}
	}

	void Move(Vector3 vel) {
		controller.Move (vel);
		// Move the controller
		if (IsHardGrounded () && !wishJump && controller.velocity.y <= 0) {
			playerVelocity.y = 0;
			controller.transform.position = new Vector3 (controller.transform.position.x, hardGroundPoint.y + controller.skinWidth + (controller.height / 2), controller.transform.position.z);
		}
	}
}