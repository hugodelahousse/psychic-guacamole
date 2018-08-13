using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour {

	public enum state {
		FIXED,
		HELD,
		PUSHED,
	}
	public state currentState = state.FIXED;

	private float timePushed = 0;
	private bool highlighted_ = false;

	public Sprite[] spritePool;
	public bool randomSprite;

	public float pushSpeed;
	public float grabbedSpeed;
	public int projectileLayer;
	public GameObject destroyParticles;
	public SpriteRenderer selectorRenderer;

    private Color startColor;
	private Rigidbody2D rb2d;
	private SpriteRenderer renderer;

	private PlayerScript owner;
	private BoxCollider2D c2d;
	public bool highlighted {
		get {
			return highlighted_;
		}
		set {
			selectorRenderer.enabled = value;
			highlighted_ = value;
		}
	}
	// Use this for initialization

	void Start () {
		renderer = GetComponent<SpriteRenderer>();
		c2d = GetComponent<BoxCollider2D>();
		rb2d = GetComponent<Rigidbody2D>();

		if (randomSprite) renderer.sprite = spritePool[Random.Range(0, spritePool.Length)];
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (currentState == state.HELD) {
            Vector2 direction = (owner.grabbedRocksPosition.position - transform.position).normalized;
			float distance = Vector2.Distance(transform.position, owner.grabbedRocksPosition.position);
            rb2d.velocity = direction * grabbedSpeed * distance;
		}
	}

	private void reduceColliderSize() {
		c2d.offset += Vector2.one * c2d.size / 10;
		c2d.size = c2d.size * 0.8f;
	}

	public void getPushed(Vector2 direction) {
		this.currentState = state.PUSHED;
		gameObject.layer = projectileLayer;
		rb2d.bodyType = RigidbodyType2D.Dynamic;
		rb2d.velocity = pushSpeed * direction;
		c2d.isTrigger = false;
		timePushed = Time.time;
	}

	public bool getGrabbed(PlayerScript script) {
		if( currentState != state.FIXED)
            return false;
		
		currentState = state.HELD;
		rb2d.bodyType = RigidbodyType2D.Dynamic;
		reduceColliderSize();
		owner = script;
		c2d.isTrigger = true;

		return true;
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (currentState != state.PUSHED || timePushed + 0.01f > Time.time) {
			return;
		}
		// Get destroyed
		Instantiate(destroyParticles, transform.position, destroyParticles.transform.rotation);
        Destroy(gameObject);
	}
}
