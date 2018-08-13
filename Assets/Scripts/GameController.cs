using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {


	public LayerMask groundLayerMask;

	public int[] playerLives;
	public GameObject[] playerPrefab;
	[SerializeField] string[] scenes;
	[SerializeField] AudioClip respawnClip;

	bool gameStarted = false;

	Vector2 findSpawnPoint() {
		List<Vector3> available = new List<Vector3>();
		foreach(GameObject rockObject in GameObject.FindGameObjectsWithTag("Rock"))
		{
			RockScript rock = rockObject.GetComponent<RockScript>();
			if (!rock)
                continue;
			Vector2 rockTop = rock.getTop();
			RaycastHit2D hit = Physics2D.Raycast(rockTop, Vector2.up, 2.5f, groundLayerMask);
			if (!hit.collider)
			{
				available.Add(rockTop);
			}
		}
		if (available.Count == 0)
            return Vector3.zero;
		return available[Random.Range(0, available.Count)];
	}

	// Use this for initialization
	void Start () {
		findSpawnPoint();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		if (!gameStarted && Input.anyKey) {
			gameStarted = true;
			GameObject.FindGameObjectWithTag("MainMenu").SetActive(false);
		}
	}

	IEnumerator RespawnPlayer(int playerIndex) {
        Vector2 spawn = findSpawnPoint();
		AudioSource.PlayClipAtPoint(respawnClip, spawn, 1f);
		yield return new WaitForSeconds(0.5f);
        GameObject newPlayer = Instantiate(playerPrefab[playerIndex], spawn + Vector2.up * 2f, Quaternion.identity);
        GameEye2D.Focus.F_Transform F_Transform = newPlayer.GetComponent<GameEye2D.Focus.F_Transform>();
        Camera.main.GetComponent<Camera2D>().AddFocus(F_Transform);
	}

	public void onPlayerDie(int playerIndex) {
		playerIndex = playerIndex - 1;
		int otherPlayerIndex = (playerIndex + 1) % 2;
		Debug.Log(string.Format("Lives: player1: {0}\tplayer2: {1}", playerLives[0], playerLives[1]));
        --playerLives[playerIndex];

		if (playerLives[otherPlayerIndex] == 0) {
			SceneManager.LoadScene(scenes[Random.Range(0, scenes.Length)]);
		}

		if (playerLives[playerIndex] == 0) {
			GameObject.FindGameObjectWithTag("Winner").GetComponent<Image>().enabled = true;
		} else {
			StartCoroutine("RespawnPlayer", playerIndex);
		}


		foreach (PlayerLives script in GameObject.FindObjectsOfType<PlayerLives>()) {
			script.onPlayerLivesChange(playerLives);
		}

	}
}
