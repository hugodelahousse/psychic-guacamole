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

    [Header("References")]
        public LayerMask groundLayer;
    public Transform groundChecker;
    public Transform rockFrontChecker;
    public Transform rockBackChecker;
    public Transform grabbedRocksPosition;

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

        if(grounded && lastJumpTime + 0.1f < Time.time && Input.GetButtonDown("Jump"))
        {
            moveVertical = jumpForce;
            lastJumpTime = Time.time;
        }

        if (Input.GetButtonDown("Punch"))
        {
            bool punched = Punch();
            if (!punched)
                Grab();
        }

        /*
           Vector2 movement = new Vector2(moveHorizontal, moveVertical);
           rb2d.AddForce(movement * speed);
           rb2d.velocity = new Vector2(
           Mathf.Min(maxSpeed.x, rb2d.velocity.x),
           Mathf.Min(maxSpeed.y, rb2d.velocity.y)
           );
           */
    }

    bool Grab() {
        if (grabbedRock) return false;
        Collider2D rockBack = Physics2D.OverlapCircle(rockBackChecker.position, 0.25f, groundLayer);
        if (!rockBack) return false;

        RockScript rockScript = rockBack.GetComponent<RockScript>();
        if (!rockScript) return false;
        if (rockScript.getGrabbed(this))
            grabbedRock = rockScript;

        return true;
    }

    Vector2 getAimingDirection() {
        return Vector2.right;
    }

    bool Punch() {
        RockScript rockScript;
        if (grabbedRock)
        {
            rockScript = grabbedRock;
            grabbedRock = null;
        }
        else {
            Collider2D rockFront = Physics2D.OverlapCircle(rockFrontChecker.position, 0.25f, groundLayer);
            if (!rockFront) return false;
            rockScript = rockFront.GetComponent<RockScript>();
        }
        if (!rockScript) return false;
        rockScript.getPushed(getAimingDirection());
        return true;
    }

}
