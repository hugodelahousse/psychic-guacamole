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

    private bool lastGrounded = false;

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

    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        getAimingDirection();
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

        if(grounded && lastJumpTime + 0.1f < Time.time && Input.GetButtonDown("Jump"))
        {
            moveVertical = jumpForce;
            lastJumpTime = Time.time;
        }

        if (Input.GetButtonDown("Punch"))
        {
            if (shouldGrab)
                Grab();
            else
                Punch();
        }

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
            aimingDirection = newDirection;

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
        RaycastHit2D backRayHit = Physics2D.Raycast(rayOrigin.position, -aimingDirection, 2f, groundLayer);
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
