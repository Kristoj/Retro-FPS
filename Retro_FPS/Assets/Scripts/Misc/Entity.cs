using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {

	protected Rigidbody rig;

	public virtual void Start() {
		rig = GetComponent<Rigidbody> ();
	}

	public virtual void OnEntityHit (float damage = default(float), Vector3 force = default(Vector3)) {
		AddForce (force);
	}

	public void AddForce(Vector3 force) {
		if (rig != null) {
			rig.AddForce (force);
		}
	}
}
