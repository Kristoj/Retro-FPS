using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity {

	[Header ("Movement")]
	public float moveSpeed = 5f;
	public float acceleration = 15f;

	[Header ("Navigation")]
	public float pathUpdateRate = .2f;
	protected bool isActive = true;
	protected Transform target;

	[Header ("Combat")]
	public float damage = 100f;

	// Classes
	protected NavMeshAgent agent;

	public override void Start() {
		base.Start();
		agent = GetComponent<NavMeshAgent> ();
		rig = GetComponent<Rigidbody> ();
		target = GameManager.GetPlayer ().transform;
		StartCoroutine (PathUpdateCoroutine ());
	}

	void FixedUpdate() {
		if (target != null) {
			PerformMovement ();
		}
	}

	public virtual void PerformMovement() {

	}

	IEnumerator PathUpdateCoroutine() {
		while (isActive) {
			OnPathUpdate ();
			yield return new WaitForSeconds (pathUpdateRate);
		}
	}

	public virtual void OnPathUpdate() {
		if (agent != null && target != null) {
			agent.SetDestination (target.position);
		}
	}

	void OnTriggerEnter (Collider c) {
		if (c.tag == "Player") {
			c.GetComponent<Player> ().TakeDamage (damage);
		}
	}
}
