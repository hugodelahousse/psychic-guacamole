using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {


	public LayerMask groundLayerMask;

	public int[] playerLives;
	public GameObject[] playerPrefab;

	

	IEnumerator Test() {
		yield return new WaitForSeconds(1f);
		findSpawnPoint();
	}

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
		
	}

	public void onPlayerDie(int playerIndex) {
		playerIndex = playerIndex - 1;
		Debug.Log(string.Format("Lives: player1: {0}\tplayer2: {1}", playerLives[0], playerLives[1]));
        --playerLives[playerIndex];
        Vector2 spawn = findSpawnPoint();
        Instantiate(playerPrefab[playerIndex], spawn + Vector2.up * 2f, Quaternion.identity);
	}
}
