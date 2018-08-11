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
    private float lastJumpTime = 0;


    public LayerMask groundLayer;
    public Transform groundChecker;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        grounded = Physics2D.OverlapCircle(groundChecker.position, 0.25f, groundLayer);

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = 0;

        if(grounded && lastJumpTime + 0.1f < Time.time  && Input.GetButtonDown("Jump"))
            moveVertical = jumpForce;

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * speed);
        rb2d.velocity = new Vector2(
            Mathf.Min(maxSpeed.x, rb2d.velocity.x),
            Mathf.Min(maxSpeed.y, rb2d.velocity.y)
        );
		
	}

}
