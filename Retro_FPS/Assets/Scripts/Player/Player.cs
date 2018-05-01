using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : LivingEntity {

	public Camera cam;

	// Use this for initialization
	void Awake () {
		// Get gunhold reference
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).GetComponent<Camera> () != null) {
				cam = transform.GetChild (i).GetComponent<Camera> ();
			}
		}

		// Application settings
		Application.targetFrameRate = 60;
	}

	public override void Die() {
		GetComponent<PlayerController> ().enabled = false;
		GetComponent<PlayerWeaponController> ().enabled = false;
	}
}
