using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Player variables
    [Space]
    [Header("Variables")]
    public float moveSpeed;
    public float jumpSpeed;
    public float fallMultiplier;
    public float lowJumpMultiplier;

    // Player references
    private PlayerCollision collision;
    private Rigidbody2D rb;

    // Input Variables
    private float horizontalMovement;
    private float verticalMovement;
    private bool jumpInput;
    private bool jumpHold;
    private Vector2 moveDirection;


    private void Awake() {
        collision = GetComponent<PlayerCollision>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    private void GetInput() {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");

        jumpInput = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
    }

    private void Walk(Vector2 move) {
        rb.velocity = new Vector2(move.x * moveSpeed * Time.deltaTime, rb.velocity.y);
    }

    private void Jump() {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.velocity += Vector2.up * jumpSpeed * Time.deltaTime;
    }

    private void GravityJumpModifier() {
        if(rb.velocity.y < 0 || (rb.velocity.y > 0 && !jumpHold))
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
    }

    private void Update() {
        GetInput();

        moveDirection = new Vector2(horizontalMovement, verticalMovement);
        Walk(moveDirection);

        if(jumpInput && collision.onGround)
            Jump();

        GravityJumpModifier();
    }

}
