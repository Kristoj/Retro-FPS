using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

	public enum FireMode {
		Auto, 
		Semi
	}

	[Header ("Weapon Stats")]
	public FireMode fireMode;
	public float damage = 20f;
	public int projectileCount = 1;
	public float rpm = 666;
	public float projectileSpeed = 70;
	public float impactForce = 500f;
	public float range = 50f;

	[Header ("Spread")]
	public Vector2 spread = new Vector2 (2.5f, 2.5f);

	[Header ("Recoil")]
	public Vector3 recoil = new Vector3 (.1f, .15f, .1f);
	public float recoilSpeed = 40f;
	public float recoilReturnSpeed = 15f;

	[Header ("Kickback")]
	public Vector3 kickback = new Vector3 (.02f, .04f, .7f);
	public float kickbackSpeed = 10f;
	public float kickbackReturnSpeed = 20f;

	[Header ("Weapon Properties")]
	public Projectile projectile;
	public LayerMask hitMask;

	// Classes
	private PlayerWeaponController weaponController;
	private PlayerController playerController;

	// Vars
	private float lastShootTime;

	public void ShootPrimary() {
		// SEMI Firemode
		if (fireMode == FireMode.Semi && !weaponController.mouseLeftReleased) {
			return;
		}
		// Shoot
		if (CanShoot()) {
			for (int i = 0; i < projectileCount; i++) {
				if (projectile == null) {
					HitScan ();
				} else {
					SpawnProjectile ();
				}
			}
			lastShootTime = Time.time;
			OnWeaponFire ();
		}
	}

	/// <summary>
	/// This is called when player fires a weapon.
	/// </summary>
	public virtual void OnWeaponFire() {
		// Add kickback
		weaponController.AddKickback (kickback);
		// Add recoil
		weaponController.AddRecoil (recoil);
		// Add camera kick
		playerController.AddCameraOffset (-kickback * .35f);

		// Audio
		AudioManager.instance.PlayCustomSound2D ("Shoot_Can", 1, false);
	}

	void HitScan() {
		Ray ray = new Ray (Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, range, hitMask, QueryTriggerInteraction.Ignore)) {
			// Take damage
			Debug.Log (hit.collider.name);
		}
	}

	void SpawnProjectile() {
		Projectile clone = Instantiate (projectile, transform.position, transform.rotation) as Projectile;
		float rngX = Random.Range (-spread.y, spread.y);
		float rngY = Random.Range (-spread.x, spread.x);
		clone.transform.eulerAngles += new Vector3 (rngX, rngY, 0);
		clone.SetupProjectile (damage, projectileSpeed, 10, impactForce, hitMask);
	}

	public void OnWeaponEquip(GameObject sourcePlayer) {
		weaponController = sourcePlayer.GetComponent<PlayerWeaponController> ();
		playerController = sourcePlayer.GetComponent<PlayerController> ();
	}


	bool CanShoot() {
		if (Time.time > lastShootTime + (60 / rpm)) {
			return true;
		} else {
			return false;
		}
	}
}
