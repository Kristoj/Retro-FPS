using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour {

	public float spawnRate = 1f;
	public bool canSpawn = true;

	public Enemy[] enemiesToSpawn;

	// Use this for initialization
	void Start () {
		StartCoroutine (SpawnCoroutine ());
	}

	IEnumerator SpawnCoroutine() {
		while (canSpawn) {
			SpawnEnemy ();
			yield return new WaitForSeconds (spawnRate);
		}
	}

	void SpawnEnemy() {
		if (enemiesToSpawn.Length > 0) {
			Enemy clone = Instantiate (enemiesToSpawn [Random.Range (0, enemiesToSpawn.Length)], transform.position, transform.rotation) as Enemy;
			clone.deathEvent += RemoveEnemy;
			GameManager.EnemyCountAdd (1);
		}
	}

	void RemoveEnemy() {
		GameManager.EnemyCountAdd (-1);
	}
}
