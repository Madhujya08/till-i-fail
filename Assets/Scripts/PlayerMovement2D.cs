using System.Collections;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float groundAcceleration = 30f;
    [SerializeField] private float airAcceleration = 12f;
    [SerializeField] private float airDeceleration = 10f;

    [Header("Jump")]
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float desiredJumpHeight = 3f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Fall")]
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.25f;
    [SerializeField] private bool dashInAirOnly = false;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckExtra = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool drawGroundRay = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private bool isFacingRight = true;

    private int jumpsRemaining;
    private bool isDashing;
    private bool wasGrounded;

    private float lastOnGroundTime;
    private float lastJumpPressedTime;
    private float baseGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 4f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        jumpsRemaining = maxJumps;

        baseGravity = -Physics2D.gravity.y * rb.gravityScale;
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);

        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpPressedTime = jumpBufferTime;                                                //buffer jump input
        else
            lastJumpPressedTime -= Time.deltaTime;


        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)                                                  //variable jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        if (Input.GetKeyDown(KeyCode.LeftShift)&& !isDashing)
        {
            if (!dashInAirOnly || (dashInAirOnly && !isGrounded()))                                                     //dash 
                StartCoroutine(Dash());
        }

        bool grounded = isGrounded();

        if (grounded) lastOnGroundTime = coyoteTime;                        //coyote time 
        else lastOnGroundTime -= Time.deltaTime;

        if (grounded && !wasGrounded)                                       //recharging the jump
        {
            jumpsRemaining = maxJumps;
        }

        if (lastJumpPressedTime > 0f && jumpsRemaining > 0f && (grounded || lastOnGroundTime > 0f || jumpsRemaining < maxJumps))
        {
            PerformJump();
            lastJumpPressedTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            bool grounded = isGrounded();
            float targetX = moveInput.x * moveSpeed;

            float accel = grounded ? groundAcceleration : airAcceleration;
            float decel = grounded ? groundAcceleration : airDeceleration;

            float newX = rb.linearVelocity.x;

            if (Mathf.Abs(moveInput.x) > 0.01f)
                newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, accel * Time.fixedDeltaTime);
            else
                newX = Mathf.MoveTowards(rb.linearVelocity.x, 0f, decel * Time.fixedDeltaTime);


            Vector2 velocity = rb.linearVelocity;
            velocity.x = newX;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
    }

    private void PerformJump()
    {
        float jumpVel = Mathf.Sqrt(2f * baseGravity * Mathf.Max(0.01f, desiredJumpHeight));

        jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);
        lastOnGroundTime = 0f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        float originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;

        float inputX = Mathf.Sign(moveInput.x);
        if (Mathf.Abs(moveInput.x) < 0.01f)
            inputX = isFacingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(inputX * dashSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravityScale;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
    }

    private bool isGrounded()
    {
        Bounds bounds = col.bounds;
        Vector2 origin = bounds.center;
        float distance = bounds.extents.y + groundCheckExtra;
        return Physics2D.Raycast(origin, Vector2.down, groundMask);
    }
}
