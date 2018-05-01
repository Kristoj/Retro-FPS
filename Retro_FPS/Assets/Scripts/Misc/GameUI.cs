using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

	public Text scoreText;
	public Text enemyCountText;

	public static GameUI instance;

	void Awake() {
		instance = this;
	}

	public void ScoreUpdate() {
		if (scoreText != null) {
			scoreText.text = "Score " + GameManager.score;
		}
	}

	public void EnemyCountUpdate() {
		if (scoreText != null) {
			enemyCountText.text = "Enemies " + GameManager.enemyCount;
		}
	}
}
