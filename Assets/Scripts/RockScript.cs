using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour {

	enum state {
		FIXED,
		HELD,
		PUSHED,
	}
	private state currentState = state.FIXED;

	public float pushSpeed;

	Rigidbody2D rb2d;
	// Use this for initialization

	void Start () {
		rb2d = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void getPushed(Vector2 direction) {
		this.currentState = state.PUSHED;
		rb2d.bodyType = RigidbodyType2D.Dynamic;
		rb2d.velocity = pushSpeed * direction;
	}
}
