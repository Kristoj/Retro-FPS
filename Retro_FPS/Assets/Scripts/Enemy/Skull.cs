using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skull : Enemy {

	[Header("Skull Stats")]
	public float turnSpeed = 15f;

	// Movement
	public override void PerformMovement() {
		if (rig != null) {
			// Move and clamp velocity
			rig.AddForce (transform.forward * acceleration, ForceMode.Acceleration);
			rig.velocity = new Vector3 (Mathf.Clamp (rig.velocity.x, -moveSpeed, moveSpeed), Mathf.Clamp (rig.velocity.y, -moveSpeed, moveSpeed), Mathf.Clamp (rig.velocity.z, -moveSpeed, moveSpeed));

			// Rotate towards target
			Quaternion targetRotation = Quaternion.LookRotation (target.position + new Vector3 (0, 1, 0) - transform.position);
			transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
		}
	}
}
