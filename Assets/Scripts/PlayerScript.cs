using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

	enum state {
        UNDECIDED,
	}

	private state currentState = state.UNDECIDED;
    private Rigidbody2D rb2d;
    private bool grounded = false;
    private float lastJumpTime = 0;

    [Header("Movement settings")]
    public float speed;
    public Vector2 maxSpeed;
    public float jumpForce;


    [Header("References")]
    public LayerMask groundLayer;
    public Transform groundChecker;
    public Transform rockFrontChecker;

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
        {
            moveVertical = jumpForce;
            lastJumpTime = Time.time;
        }

        if (Input.GetButtonDown("Punch"))
        {
            Punch();
        }

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        rb2d.AddForce(movement * speed);
        rb2d.velocity = new Vector2(
            Mathf.Min(maxSpeed.x, rb2d.velocity.x),
            Mathf.Min(maxSpeed.y, rb2d.velocity.y)
        );
		
	}


    void Punch() {
        Collider2D rockFront = Physics2D.OverlapCircle(rockFrontChecker.position, 0.25f, groundLayer);
        if (rockFront)
        {
            RockScript rockScript = rockFront.GetComponent<RockScript>();
            if (rockScript)
            {
                rockScript.getPushed(new Vector2(
                    rockFront.transform.position.x > transform.position.x ? 1 : -1, 0
                ));
            }
        }
    }

}
