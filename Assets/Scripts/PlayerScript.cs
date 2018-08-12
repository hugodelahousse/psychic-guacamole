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
	private bool lastGrounded = false;
	private float lastJumpTime = 0;

	private SpriteRenderer sr;
	private bool facingLeft = false;

    private RockScript grabbedRock = null;

    [Header("Movement settings")]
        public float speed;
    public Vector2 maxSpeed;
    public float jumpForce;

    private Animator anim;
    private Vector2 aimingDirection;
    private RockScript selectedRock = null;
    private bool shouldGrab = false;

    [Header("References")]
    public LayerMask groundLayer;
    public Transform groundChecker;

    public Transform rayOrigin;
    public Transform grabbedRocksPosition;

    private bool stunned;

	float moveVertical = 0;
	float moveHorizontal = 0;

    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		sr = GetComponent<SpriteRenderer>();
        getAimingDirection();
    }

	private void Update()
	{
		sr.flipX = facingLeft;

		moveVertical = 0;

		if (grounded && lastJumpTime + 0.1f < Time.time && Input.GetButtonDown("Jump"))
		{
			moveVertical = jumpForce;
			rb2d.AddForce(new Vector2(0, moveVertical) * speed);
			lastJumpTime = Time.time;
		}

		if (grounded) anim.SetFloat("Velocity", Mathf.Abs(moveHorizontal));

		if (Input.GetButtonDown("Punch"))
		{
			if (shouldGrab && !grabbedRock)
				Grab();
			else
				Punch();
		}
	}

	// Update is called once per frame
	void FixedUpdate() {	
	
        grounded = Physics2D.OverlapCircle(groundChecker.position, 0.25f, groundLayer);
		if (lastGrounded != grounded) anim.SetBool("Jump", lastGrounded);
        if (lastGrounded == false && grounded == true) anim.SetTrigger("Landing");
        lastGrounded = grounded;

        moveHorizontal = Input.GetAxisRaw("Horizontal");

		if (moveHorizontal > 0) facingLeft = false;
		else if (moveHorizontal < 0) facingLeft = true;

        rb2d.velocity = stunned ? new Vector2(rb2d.velocity.x, rb2d.velocity.y) : new Vector2(moveHorizontal * speed, rb2d.velocity.y);

        getAimingDirection();
        HighlightSelectedRock();
    }

    bool Grab() {
        RockScript rockScript = selectedRock;
        if (!rockScript) return false;
        if (rockScript.getGrabbed(this))
            grabbedRock = rockScript;

        return true;
    }

    Vector2 getAimingDirection() {
        Vector2 newDirection = new Vector2(Input.GetAxisRaw("Horizontal"),
                                        Input.GetAxisRaw("Vertical"));
        if (newDirection.magnitude > 0.2f)
            aimingDirection = newDirection.normalized;
		else
			aimingDirection = facingLeft ? new Vector2(-1, 0) : new Vector2(1, 0);

        return aimingDirection;
    }

    void HighlightSelectedRock() {
        RockScript script = null;
        RaycastHit2D frontRayHit = Physics2D.Raycast(rayOrigin.position, aimingDirection, 2f, groundLayer);
        if (frontRayHit) {
            script = frontRayHit.collider.GetComponent<RockScript>();
            if (script) {
                setSelectedRock(script);
                shouldGrab = false;
                return;
            }
        }
        RaycastHit2D backRayHit = Physics2D.Raycast(rayOrigin.position, -aimingDirection, 10f, groundLayer);
        if (backRayHit) {
            script = backRayHit.collider.GetComponent<RockScript>();
            if (script) {
                setSelectedRock(script);
                shouldGrab = true;
                return;
            }
        }
        setSelectedRock(null);
    }

    void setSelectedRock(RockScript rock) {
        if (rock == selectedRock)
            return;
        if (selectedRock)
            selectedRock.highlighted = false;
        if (rock)
        {
            selectedRock = rock;
            selectedRock.highlighted = true;
        }
    }

    bool Punch() {
        RockScript rockScript;
        if (grabbedRock)
        {
            rockScript = grabbedRock;
            grabbedRock = null;
        }
        else {
            rockScript = selectedRock;
        }
        if (!rockScript) return false;
        rockScript.getPushed(getAimingDirection());
        return true;
    }

}
