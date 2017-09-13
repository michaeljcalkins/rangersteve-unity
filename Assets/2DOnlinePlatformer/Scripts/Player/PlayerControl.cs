﻿using UnityEngine;

/* Network Description

to Update, FixedUpdate, Start the other players do not come because they disable the script
(because they are not the owner of this prefab "hero" (which has this script - PlayerControl) which was created in OnJoinedRoom(script ConnectRoom))

Network Description */
public class PlayerControl : MonoBehaviour {
    // For determining which way the player is currently facing.
    [HideInInspector]
    public bool facingRight = true;

    // Condition for whether the player should jump.
    [HideInInspector]
    public bool jump = false;

    // Condition for whether the player should sit.
    [HideInInspector]
    public bool sit = false;

    // Condition for whether the player should fly.
    [HideInInspector]
    public bool flying = false;

    // Amount of force added to move the player left and right.
    public float moveForce = 365f;

    // The fastest the player can travel in the x axis.
    public float maxSpeedX = 10f;

    // The fastest the player can travel in the x axis.
    public float maxSpeedY = 20f;

    // Array of clips for when the player jumps.
    public AudioClip[] jumpClips;

    // Amount of force added when the player jumps.
    public float jumpForce = 1200f;

    // Amount of force added when the player flys.
    public float flyingForce = 150f;

    // A position marking where to check if the player is grounded.
    private Transform groundCheck;

    // Whether or not the player is grounded.
    private bool grounded = false;

    // Reference to the player's animator component.
    private Animator anim;

    void Start () {
        // Setting up references.
        groundCheck = transform.Find ("groundCheck");
        anim = GetComponent<Animator> ();

        // Make the camera follow the player
        Camera.main.GetComponent<CameraFollow> ().setTarget (gameObject.transform);
    }

    void Update () {
        // The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
        grounded = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground"));

        // If the jump button is pressed and the player is grounded then the player should jump.
        if (Input.GetButtonDown ("Jump") && grounded) {
            jump = true;
        }

        if (Input.GetButtonDown ("Down")) {
            sit = true;
        }

        if (Input.GetButtonUp ("Down")) {
            sit = false;
        }

        flying = Input.GetMouseButton (1);
    }

    void FixedUpdate () {
        float h = 0;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo (0);
        if (!stateInfo.IsName ("Base Layer.Sit")) {
            // Cache the horizontal input.
            h = Input.GetAxis ("Horizontal");
        }

        // The Speed animator parameter is set to the absolute value of the horizontal input.
        anim.SetFloat ("Speed", Mathf.Abs (h));

        // If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeedX yet...
        if (h * GetComponent<Rigidbody2D> ().velocity.x < maxSpeedX) {
            // ... add a force to the player.
            GetComponent<Rigidbody2D> ().AddForce (Vector2.right * h * moveForce);
        }

        // If the player should fly...
        if (flying && Mathf.Abs (GetComponent<Rigidbody2D> ().velocity.y) < maxSpeedY) {
            // Set the Jump animator trigger parameter.
            anim.SetTrigger ("Jump");

            // Play a random jump audio clip.
            int i = Random.Range (0, jumpClips.Length);
            AudioSource.PlayClipAtPoint (jumpClips[i], transform.position);

            // Add a vertical force to the player.
            GetComponent<Rigidbody2D> ().AddForce (new Vector2 (0f, flyingForce));
        }

        // If the player's horizontal velocity is greater than the maxSpeedX...
        if (Mathf.Abs (GetComponent<Rigidbody2D> ().velocity.x) > maxSpeedX) {
            // ... set the player's velocity to the maxSpeedX in the x axis.
            GetComponent<Rigidbody2D> ().velocity = new Vector2 (Mathf.Sign (GetComponent<Rigidbody2D> ().velocity.x) * maxSpeedX, GetComponent<Rigidbody2D> ().velocity.y);

            // If the input is moving the player right and the player is facing left...
            if (h > 0 && !facingRight) {
                // ... flip the player.
                Flip ();
            } else if (h < 0 && facingRight) {
                // Otherwise if the input is moving the player left and the player is facing right...flip the player.
                Flip ();
            }

            // If the player should jump...
            if (jump) {
                // Set the Jump animator trigger parameter.
                anim.SetTrigger ("Jump");

                // Play a random jump audio clip.
                int i = Random.Range (0, jumpClips.Length);
                AudioSource.PlayClipAtPoint (jumpClips[i], transform.position);

                // Add a vertical force to the player.
                GetComponent<Rigidbody2D> ().AddForce (new Vector2 (0f, jumpForce));

                // Make sure the player can't jump again until the jump conditions from Update are satisfied.
                jump = false;
            }

            anim.SetBool ("Sit", sit);
        }
    }

    void Flip () {
        // Switch the way the player is labelled as facing.
        facingRight = !facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}