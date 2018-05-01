using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : Entity {

	public delegate void DeathEvent();
	public DeathEvent deathEvent;

	public float startingHealth = 100f;
	[HideInInspector]
	public bool isDead = false;
	protected float health;

	public override void Start() {
		base.Start ();
		health = startingHealth;
	}

	public override void OnEntityHit (float damage, Vector3 force) {
		base.OnEntityHit (damage, force);
		TakeDamage (damage);
	}

	public void TakeDamage(float damage) {
		if (!isDead) {
			health -= damage;

			if (health <= 0) {
				isDead = true;
				Die ();
			}
		}
	}

	public virtual void Die() {
		OnDeath ();
		Destroy (gameObject);
	}

	public virtual void OnDeath() {
		GameManager.ScoreAdd (1);

		if (deathEvent != null) {
			deathEvent ();
			deathEvent = null;
		}
	}
}
