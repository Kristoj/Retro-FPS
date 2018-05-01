using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static int score;
	public static int enemyCount;

	public static GameManager instance;
	private static Player player;

	// Use this for initialization
	void Awake () {
		instance = this;
		SetupReferences ();
	}

	void SetupReferences() {
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>();
	}

	public static Player GetPlayer() {
		return player;
	}

	public static void ScoreAdd (int amount) {
		score += amount;
		GameUI.instance.ScoreUpdate ();
	}

	public static void EnemyCountAdd (int amount) {
		enemyCount += amount;
		GameUI.instance.EnemyCountUpdate ();
	}
}
