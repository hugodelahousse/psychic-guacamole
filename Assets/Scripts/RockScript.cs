using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour {

	public enum state {
		FIXED,
		HELD,
		PUSHED,
	}

	[SerializeField] bool isBig;
	public state currentState = state.FIXED;

	private float timePushed = 0;
	private int highlighted_ = 0;

	[SerializeField] AudioClip destroySound;
	public Sprite[] spritePool;
	public bool randomSprite;

	public float pushSpeed;
	public float grabbedSpeed;
	public int projectileLayer;
	public GameObject destroyParticles;
	public SpriteRenderer[] selectorRenderers;
	public float smallForceRadius;
	public float constantForceRadius;
	public float smallForce;
	public float constantForce;
	public float destroyTime;

    private Color startColor;
	private Rigidbody2D rb2d;
	private SpriteRenderer renderer;

	private KinematicPlayer owner;

	bool onlyOnce = false;

	Collider2D[] aimAssistTarget = new Collider2D[16];
	public ContactFilter2D contactFilter;

	// public so we can do physics2d.ignore collision in the player punch function
	[HideInInspector]
	public BoxCollider2D c2d;

	public int highlighted {
		get {
			return highlighted_;
		}
		set {
            selectorRenderers[0].enabled = false;
            selectorRenderers[1].enabled = false;
			if (value != 0) {
                selectorRenderers[value - 1].enabled = true;
			}
			highlighted_ = value;
		}
	}
	// Use this for initialization

	void Awake () {
		renderer = GetComponent<SpriteRenderer>();
		c2d = GetComponent<BoxCollider2D>();
		rb2d = GetComponent<Rigidbody2D>();

		if (randomSprite) renderer.sprite = spritePool[Random.Range(0, spritePool.Length)];
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (currentState == state.HELD) {
			if (!owner)
			{
				Destroy(gameObject);
				return;
			}
			
			Vector3 centerPosition = c2d.offset;
			centerPosition += transform.position;

            Vector2 direction = (owner.grabbedRocksPosition.position - centerPosition).normalized;
			float distance = Vector2.Distance(centerPosition, owner.grabbedRocksPosition.position);
            rb2d.velocity = direction * grabbedSpeed * distance;
		}

		if (currentState == state.PUSHED)
		{
			Vector3 centerPosition = c2d.offset;
			centerPosition += transform.position;

			int smallForceCount = Physics2D.OverlapCircle(centerPosition, smallForceRadius, contactFilter, aimAssistTarget);

			for (int i = 0; i < smallForceCount; i++)
			{
				if (aimAssistTarget[i].gameObject != owner.gameObject && aimAssistTarget[i].CompareTag("Player") && !onlyOnce)
				{
					//Debug.DrawRay(centerPosition, (aimAssistTarget[i].transform.position - transform.position).normalized * smallForce, Color.white, 5f);
					rb2d.AddForce((aimAssistTarget[i].transform.position - centerPosition).normalized * smallForce, ForceMode2D.Impulse);
					rb2d.velocity = rb2d.velocity.normalized * pushSpeed;
					onlyOnce = true;
				}
			}

			int conForceCount = Physics2D.OverlapCircle(centerPosition, constantForceRadius, contactFilter, aimAssistTarget);

			for (int i = 0; i < conForceCount; i++)
			{
				if (aimAssistTarget[i].gameObject != owner.gameObject && aimAssistTarget[i].CompareTag("Player"))
				{
					//Debug.DrawRay(centerPosition, (aimAssistTarget[i].transform.position - transform.position).normalized * constantForce, Color.red, 5f);
					rb2d.AddForce((aimAssistTarget[i].transform.position - centerPosition).normalized * constantForce, ForceMode2D.Impulse);
					rb2d.velocity = rb2d.velocity.normalized * pushSpeed;
				}
			}
		}

		if (timePushed != 0 && timePushed + destroyTime < Time.time)
		{
			Destroy(gameObject);
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

	public bool getGrabbed(KinematicPlayer script) {
		if( currentState != state.FIXED)
            return false;
		
		currentState = state.HELD;
		rb2d.bodyType = RigidbodyType2D.Dynamic;
		reduceColliderSize();
		owner = script;
		c2d.isTrigger = true;

		return true;
	}

	public Vector2 getTop() {
		return new Vector2(transform.position.x + c2d.offset.x, transform.position.y + 0.1f);
	}

	IEnumerator DestroyNextFrame() {
		yield return new WaitForEndOfFrame();
		Destroy(gameObject);
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (currentState != state.PUSHED) {
			return;
		}
		
		//Debug.Log("Hit something: " + other.gameObject);
		if (other.gameObject.CompareTag("Player")) {
			FindObjectOfType<GameController>().freezeFrame();
			other.gameObject.GetComponent<KinematicPlayer>().GetHit(other.relativeVelocity);
		}

		if (other.gameObject.CompareTag("Rock")) {
			Instantiate(destroyParticles, transform.position, destroyParticles.transform.rotation);
			Destroy(other.gameObject);
		}

		// Get destroyed
		Camera.main.GetComponent<CameraShake>().shake(isBig);
		Instantiate(destroyParticles, transform.position, destroyParticles.transform.rotation);
		//StartCoroutine("DestroyNextFrame");
		Destroy(gameObject);
		AudioSource.PlayClipAtPoint(destroySound, transform.position, 10f);
	}

	private void OnDrawGizmosSelected()
	{
		UnityEditor.Handles.color = Color.green;
		UnityEditor.Handles.DrawWireDisc(new Vector2(transform.position.x, transform.position.y) + GetComponent<Collider2D>().offset, Vector3.back, smallForceRadius);
		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawWireDisc(new Vector2(transform.position.x, transform.position.y) + GetComponent<Collider2D>().offset, Vector3.back, constantForceRadius);
	}
}
