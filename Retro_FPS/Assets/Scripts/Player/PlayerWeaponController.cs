using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour {

	[Header ("Weapon Slots")]
	public Weapon weapon01;
	public Weapon weapon02;
	public Weapon curWeapon;
	private Transform gunHold;
	private Vector3 ghOriginalPos;

	[Header ("Sway")]
	public float swayAcceleration = .25f;
	public float swayScaleX = .1f;
	public float swayScaleY = .1f;
	public float swayScaleZ = .1f;
	public float swaySpeed = 2f;
	public float swayReturnSpeed = 10f;

	// Vectors
	private Vector3 targetSwayVector;
	private Vector3 targetKickbackVector;
	private Vector3 targetRecoilVector;
	private Vector3 swayVector;
	private Vector3 kickbackVector;
	private Vector3 recoilVector;


	// Classes
	private Player player;
	private PlayerController playerController;

	// Input
	public bool mouseLeftDown;
	public bool mouseLeftReleased;
	public bool mouseRightDown;
	public bool mouseRightReleased;

	// Use this for initialization
	void Start () {

		// Class references
		player = GetComponent<Player>();
		playerController = GetComponent<PlayerController> ();

		// Get gunhold reference
		for (int i = 0; i < player.cam.transform.childCount; i++) {
			if (player.cam.transform.GetChild (i).name == "GunHold") {
				gunHold = player.cam.transform.GetChild (i);
				ghOriginalPos = gunHold.transform.localPosition;
			}
		}


		// If we have a weapon in slot spawn one at the start
		if (weapon01 != null) 
			ChangeWeapon (weapon01);
		if (weapon02 != null) 
			ChangeWeapon (weapon02);
	}
	
	// Update is called once per frame
	void Update () {
		CheckInput ();
		Sway ();

		if (curWeapon != null) {
			// Oriantate gun hold
			swayVector = Vector3.Lerp (swayVector, targetSwayVector, swaySpeed * Time.deltaTime);
			kickbackVector = Vector3.Lerp (kickbackVector, targetKickbackVector, curWeapon.kickbackSpeed * Time.deltaTime);
			recoilVector = Vector3.Lerp (recoilVector, targetRecoilVector, curWeapon.recoilSpeed * Time.deltaTime);
			// Apply transform
			gunHold.transform.localPosition = ghOriginalPos + swayVector + kickbackVector;
			playerController.recoilEuler = recoilVector;


			// Return GH vectors to original pos overtime
			targetSwayVector = Vector3.Lerp (targetSwayVector, Vector3.zero, swayReturnSpeed * Time.deltaTime);
			targetKickbackVector = Vector3.Lerp (targetKickbackVector, Vector3.zero, curWeapon.kickbackReturnSpeed * Time.deltaTime);
			targetRecoilVector = Vector3.Lerp (targetRecoilVector, Vector3.zero, curWeapon.recoilReturnSpeed * Time.deltaTime);
		}
	}

	/// <summary>
	/// Changes current weapon to the given weapon.
	/// </summary>
	/// <param name="weaponToEquip">Weapon to equip.</param>
	public void ChangeWeapon(Weapon weaponToEquip) {
		// Destroy our old weapon if one exists
		DestroyCurrentWeapon ();
		// Spawn new Weapon
		SpawnWeapon(weaponToEquip);
	}
		
	// Spawn the given weapon
	void SpawnWeapon(Weapon weaponToSpawn) {
		curWeapon = Instantiate (weaponToSpawn, gunHold.position, gunHold.rotation, gunHold) as Weapon;
		curWeapon.OnWeaponEquip (this.gameObject);
	}
		
	// Destroys the current weapon.
	void DestroyCurrentWeapon() {
		if (curWeapon != null) {
			Destroy (curWeapon.gameObject);
			curWeapon = null;
		}
	}

	void CheckInput() {

		// Primary Shoot
		if (Input.GetButton ("Fire1")) {
			curWeapon.ShootPrimary ();
		}

		// MOUSE STATES START -- \\
		if (Input.GetButtonDown ("Fire1")) {
			mouseLeftDown = true;
			mouseLeftReleased = false;
		}
		if (Input.GetButtonUp ("Fire1")) {
			mouseLeftDown = false;
			mouseLeftReleased = true;
		}
		if (Input.GetButtonDown ("Fire2")) {
			mouseRightDown = true;
			mouseRightReleased = false;
		}
		if (Input.GetButtonUp ("Fire2")) {
			mouseRightDown = false;
			mouseRightReleased = true;
		}
		// MOUSE STATES END -- \\

		// Weapon Changing input
		if (Input.GetKeyDown (KeyCode.Alpha1))
			ChangeWeapon (weapon01);
		if (Input.GetKeyDown (KeyCode.Alpha2))
			ChangeWeapon (weapon02);
	}

	// Sway
	void Sway() {
		// Get input
		float xInput = Input.GetAxisRaw ("Mouse X");
		float yInput = Input.GetAxisRaw ("Mouse Y");

		// Add input
		targetSwayVector.x -= xInput * swayAcceleration * Time.deltaTime;
		targetSwayVector.y -= yInput * swayAcceleration * Time.deltaTime;

		// Clamp sway vector
		targetSwayVector.x = Mathf.Clamp (targetSwayVector.x, -swayScaleX, swayScaleX);
		targetSwayVector.y = Mathf.Clamp (targetSwayVector.y, -swayScaleY, swayScaleY);
	}

	// Add kickback when firing a weapon
	public void AddKickback (Vector3 kickback) {
		float kickX = Random.Range (-kickback.x, kickback.x);
		float kickY = Random.Range (-kickback.y, kickback.y);
		float kickZ = Random.Range (kickback.z * .5f, kickback.z);
		targetKickbackVector -= new Vector3 (kickX, kickY, kickZ);
	}

	public void AddRecoil(Vector3 recoil) {
		float recoilX = Random.Range (-recoil.x, recoil.x);
		float recoilY = Random.Range (-recoil.y, recoil.y);
		float recoilZ = Random.Range (recoil.z * .5f, recoil.z);

		targetRecoilVector = new Vector3 (recoilX, recoilY, recoilZ);
	}
}
