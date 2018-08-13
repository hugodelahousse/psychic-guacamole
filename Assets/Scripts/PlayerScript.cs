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
    private bool doubleJumped = false;
	private bool facingLeft_ = false;
    private float grabbedRocksPositionX;

    private bool facingLeft {
        get {
            return facingLeft_;
        }
        set {
            facingLeft_ = value;
            sr.flipX = value;
            grabbedRocksPosition.localPosition = new Vector2(
                grabbedRocksPositionX * (value ? -1 : 1),
                grabbedRocksPosition.localPosition.y 
            );
        }
    }

    private RockScript grabbedRock = null;

    [Header("Player settings")]
    public int playerNumber = 1;
    [SerializeField] AudioClip deathClip;

    [Header("Combat settings")]
    public float stunTime;

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
        grabbedRocksPositionX = grabbedRocksPosition.localPosition.x;
        getAimingDirection();
    }

    string getPlayerKey(string keyName) {
        return string.Format("Player{0}{1}", playerNumber, keyName);
    }

	// Update is called once per frame
	void FixedUpdate() {	
        grounded = Physics2D.OverlapCircle(groundChecker.position, 0.25f, groundLayer);
		if (lastGrounded != grounded) anim.SetBool("Jump", lastGrounded);
        if (lastGrounded == false && grounded == true) anim.SetTrigger("Landing");
        lastGrounded = grounded;

        if (Input.GetButton(getPlayerKey("Aim")))
            moveHorizontal = 0;
        else
            moveHorizontal = Input.GetAxisRaw(getPlayerKey("Horizontal"));



		moveVertical = 0;

		if ((grounded || !doubleJumped) && lastJumpTime + 0.1f < Time.time && Input.GetButtonDown(getPlayerKey("Jump")))
		{
            doubleJumped = !grounded;
			moveVertical = jumpForce;
			rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
			rb2d.AddForce(new Vector2(0, moveVertical) * speed);
			lastJumpTime = Time.time;
		}

		if (grounded) anim.SetFloat("Velocity", Mathf.Abs(moveHorizontal));

        getAimingDirection();
        HighlightSelectedRock();

		if (Input.GetButtonDown(getPlayerKey("Punch")))
		{
			if (shouldGrab && !grabbedRock)
				Grab();
			else
				Punch();
		}

    }

    void LateUpdate()
    {
        if (!stunned) {
            rb2d.velocity = new Vector2(moveHorizontal * speed, rb2d.velocity.y);
        }
    }

    bool Grab() {
        RockScript rockScript = selectedRock;
        if (rockScript && rockScript.getGrabbed(this))
        {
           grabbedRock = rockScript;
           return true;
        }

        return false;
    }

    Vector2 getAimingDirection() {
        Vector2 newDirection = new Vector2(Input.GetAxisRaw(getPlayerKey("Horizontal")),
                                        Input.GetAxisRaw(getPlayerKey("Vertical")));
        if (newDirection.magnitude > 0.2f)
        {
            aimingDirection = newDirection.normalized;
            if (newDirection.x != 0) facingLeft = newDirection.x < 0;
        }
		else
		{
			aimingDirection = new Vector2(facingLeft ? -1 : 1, 0);
		}

        return aimingDirection;
    }

    void HighlightSelectedRock() {
        if (grabbedRock)
        {
            setSelectedRock(null);
            return;
        }
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
            selectedRock.highlighted = 0;
		if (rock)
		{
			selectedRock = rock;
			selectedRock.highlighted = playerNumber;
		}
		else selectedRock = null;
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
            setSelectedRock(null);
        }
        if (!rockScript) return false;
        rockScript.getPushed(getAimingDirection());
		if (grounded) anim.SetTrigger("Punch");
		else anim.SetTrigger("Punch_air");
        return true;
    }

    IEnumerator Stun() {
        yield return new WaitForSeconds(stunTime);
        stunned = false;
    }

    /// <summary>
    /// Sent when an incoming collider makes contact with this object's
    /// collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log(other.gameObject + " " + gameObject.name);
        if (other.gameObject.CompareTag("DeathCollider")) {
            FindObjectOfType<GameController>().onPlayerDie(playerNumber);
            if (gameObject)
            {
                Camera.main.GetComponent<Camera2D>().RemoveFocus(this.GetComponent<GameEye2D.Focus.F_Transform>());
                AudioSource.PlayClipAtPoint(deathClip, transform.position, 2f);
                Destroy(gameObject);
            }
            return;
        }
        RockScript rockScript = other.gameObject.GetComponent<RockScript>();
        if (!rockScript || rockScript.currentState != RockScript.state.PUSHED)
            return;
        Debug.Log("Got hit by a rock");
        if (!stunned)
        {
            stunned = true;
            StartCoroutine("Stun");
        }
    }
}
