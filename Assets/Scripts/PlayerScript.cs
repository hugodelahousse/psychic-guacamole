using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

	enum state {
        UNDECIDED,
	}
	private state currentState = state.UNDECIDED;
    private Rigidbody2D rb2d;

    public float speed;
    public Vector2 maxSpeed;
    public float jumpForce;

    private bool grounded = false;
	private bool lastGrounded = false;
    private float lastJumpTime = 0;

	private Animator anim;
	
    public LayerMask groundLayer;
    public Transform groundChecker;

	private bool stunned;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void FixedUpdate() {

		grounded = Physics2D.OverlapCircle(groundChecker.position, 0.25f, groundLayer);
		if (lastGrounded == false && grounded == true) Debug.Log("landed"); // anim.SetTrigger("Landing");
		lastGrounded = grounded;

		float moveHorizontal = Input.GetAxisRaw("Horizontal");
		//anim.SetFloat("Velocity", moveHorizontal);

		float moveVertical = 0;

		if (grounded && lastJumpTime + 0.1f < Time.time && Input.GetButtonDown("Jump"))
		{
			moveVertical = jumpForce;
			//anim.SetTrigger("Jump");
		}

		rb2d.AddForce(new Vector2(0, moveVertical) * speed);
		rb2d.velocity = stunned ? new Vector2(rb2d.velocity.x, rb2d.velocity.y) : new Vector2(moveHorizontal * speed, rb2d.velocity.y);


		/*
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * speed);
        rb2d.velocity = new Vector2(
            Mathf.Min(maxSpeed.x, rb2d.velocity.x),
            Mathf.Min(maxSpeed.y, rb2d.velocity.y)
        );
		*/
	}

}
