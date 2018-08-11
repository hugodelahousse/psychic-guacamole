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

	private float timePushed = 0;

	public float pushSpeed;
	public int projectileLayer;

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
		Debug.LogError(projectileLayer);
		gameObject.layer = projectileLayer;
		rb2d.bodyType = RigidbodyType2D.Dynamic;
		rb2d.velocity = pushSpeed * direction;
		timePushed = Time.time;
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		collider.offset += Vector2.one * collider.size / 10;
		collider.size = collider.size * 0.8f;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (currentState != state.PUSHED) {
			return;
		}
        Debug.LogError(gameObject.name + " " + other.gameObject);
        // Get destroyed
        Destroy(gameObject);
	}
}
