using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	private float lifeSpan = 10f;
	private float damage = 5f;
	private float speed = 70f;
	private float impactForce = 500f;
	private LayerMask hitMask;
	
	// Update is called once per frame
	void Update () {
		CheckCollisions ();
		MoveProjectile ();

		lifeSpan -= Time.deltaTime;
		if (lifeSpan <= 0) {
			KillProjectile ();
		}
	}

	void MoveProjectile() {
		transform.Translate (Vector3.forward * speed * Time.deltaTime);
	}

	void CheckCollisions() {
		Ray ray = new Ray (transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, 1, hitMask)) {
			// Take damage
			Entity e = hit.collider.GetComponent<Entity>();
			if (e != null) {
				e.OnEntityHit (damage, transform.forward * impactForce);
			}
			Destroy (this.gameObject);
		}
	}

	void KillProjectile() {
		Destroy (this.gameObject);
	}

	public void SetupProjectile(float newDamage, float newSpeed, float newLifeSpan, float impForce, LayerMask newHitMask) {
		damage = newDamage;
		speed = newSpeed;
		lifeSpan = newLifeSpan;
		impactForce = impForce;
		hitMask = newHitMask;
	}
}
