using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField] private int coyoteTime = 300;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Fall")]
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.25f;
    [SerializeField] private bool dashInAirOnly = false;

    [Header("Ground Probe")]
    [SerializeField] float probeHeight = 0.08f;
    [SerializeField] float probeShrink = 0.10f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] int groundedMinFrames = 2;

    [Header("Step/Unstick")]
    [SerializeField] private float stepHeight = 0.12f;
    [SerializeField] private float stepCheckDistance = 0.12f;
    [SerializeField] private float stuckSpeedThreshold = 0.02f;
    [SerializeField] private int stuckFrameToNudge = 2;
    [SerializeField] private float unstickNudgeUp = 0.02f;

    [Header("Debug")]
    [SerializeField] private bool drawGroundRay = true;

    private Rigidbody2D rb;
    private Collider2D collider;
    private SpriteRenderer sr;
    private Vector2 moveInput;
    private bool isFacingRight = true;

    private int jumpsRemaining;
    private bool isDashing;
    private bool grounded;
    private bool wasGrounded;
    private int groundedGrace;

    private long lastOnGroundTime;
    private float lastJumpPressedTime;

    private int StuckFrames;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 4f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        jumpsRemaining = maxJumps;

        
    }

    private void Update()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);

        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpPressedTime = jumpBufferTime;                                                //buffer jump input
        else
            lastJumpPressedTime -= Time.deltaTime;


        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)                                                  //variable jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        if (Input.GetKeyDown(KeyCode.LeftShift)&& !isDashing)
        {
            if (!dashInAirOnly || (dashInAirOnly && !grounded))                                                     //dash 
                StartCoroutine(Dash());
        }


        if (grounded) lastOnGroundTime = currentTime;                        //coyote time 
        
        bool canCoyoteJump =  currentTime - lastOnGroundTime < coyoteTime;

       
        if (lastJumpPressedTime > 0f && jumpsRemaining > 0 && (grounded || jumpsRemaining < maxJumps || canCoyoteJump))
        {
            if (canCoyoteJump)
            {
                Debug.Log("ct:" + currentTime);
                Debug.Log("lastonground:" + lastOnGroundTime);
                Debug.Log("CoyoteTime:" + coyoteTime);
            }
            PerformJump();
            lastJumpPressedTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        bool hit = ProbeGround();

        if (hit)
        {
            groundedGrace = groundedMinFrames;
            grounded = true;
        }
        else
        {
            if (groundedGrace > 0)
                groundedGrace--;
            else
                grounded = false;
        }

        if (grounded && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }
        wasGrounded = grounded;

        StepUpIfNeeded();
        UnstickIfNeeded();

        if (!isDashing)
        {
            float targetX = moveInput.x * moveSpeed;

            float accel = grounded ? groundAcceleration : airAcceleration;
            float decel = grounded ? groundAcceleration : airDeceleration;

            float newX;
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, accel * Time.fixedDeltaTime);
            }
            else
            {
                newX = Mathf.MoveTowards(rb.linearVelocity.x, 0f, decel * Time.fixedDeltaTime);
            }

            Vector2 velocity = rb.linearVelocity;
            velocity.x = newX;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            rb.linearVelocity = velocity;
        }

        else
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            rb.linearVelocity = velocity;
        }
    }

    private void PerformJump()
    {
        float g = -Physics2D.gravity.y * rb.gravityScale;
        float jumpVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, desiredJumpHeight));

        jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVel);
        lastOnGroundTime = 0;
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        float originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;

        float inputX = Input.GetAxisRaw("Horizontal");

        float xDir = inputX != 0 ? Mathf.Sign(inputX) : (isFacingRight ? 1f : -1f);

        rb.linearVelocity = new Vector2(xDir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravityScale;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
    }

    bool ProbeGround()
    {
        var bounds = collider.bounds;
        var size = new Vector2(bounds.size.x * (1f - probeShrink), probeHeight);
        var center = new Vector2(bounds.center.x, bounds.min.y - probeHeight * 0.5f);
        return Physics2D.OverlapBox(center, size, 0f, groundMask) != null;
    }

    private void StepUpIfNeeded()
    {
        if (!grounded || Mathf.Abs(moveInput.x) < 0.01f)
            return;

        float dir = Mathf.Sign(moveInput.x);
        Bounds b = collider.bounds;

        Vector2 feet = new Vector2(b.center.x, b.min.y + 0.02f);

        RaycastHit2D lowHit = Physics2D.Raycast(feet, Vector2.right * dir, stepCheckDistance, groundMask);

        if (!lowHit)
            return;

        Vector2 stepToStart = new Vector2(lowHit.point.x, b.min.y + stepHeight + 0.02f);
        RaycastHit2D downHit = Physics2D.Raycast(stepToStart, Vector2.down, stepHeight + 0.04f, groundMask);

        if (!downHit)
            return;

        float currentFeetY = b.min.y;
        float desiredFeetY = downHit.point.y;
        float stepSize = desiredFeetY - currentFeetY;

        if (stepSize > 0f && stepSize <= stepHeight)
        {
            rb.position += Vector2.up * stepSize;
        }
    }

    private void UnstickIfNeeded()
    {
        bool pushing = grounded && Mathf.Abs(moveInput.x) > 0.01f;
        float speedX = Mathf.Abs(rb.linearVelocity.x);

        if(pushing && speedX < stuckSpeedThreshold)
        {
            StuckFrames++;
            if (StuckFrames >= stuckFrameToNudge)
            {
                rb.position += Vector2.up * unstickNudgeUp;
                StuckFrames = 0;
            }
        }
        else
        {
            StuckFrames = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGroundRay) return;
        if (collider == null) collider = GetComponent<Collider2D>();

        Bounds b = collider.bounds;
        Vector2 size = new Vector2(b.size.x * (1f - probeShrink), probeHeight);
        Vector2 center = new Vector2(b.center.x, b.min.y - probeHeight * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }

    //private bool isGrounded()
    //{
    //    Bounds bounds = collider.bounds;
    //    Vector2 origin = bounds.center;
    //    float distance = bounds.extents.y + groundCheckExtra;
    //     return Physics2D.Raycast(origin, Vector2.down , distance,  groundMask);
    // }

    // private bool isGrounded2() 
    // { float extraHeight = 0.1f; 
    //   RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GetComponent<Collider2D>().bounds.extents.y + extraHeight, LayerMask.GetMask("Ground")); 
    //   return hit.collider != null; 
    // }




    // private void OnCollisionEnter2D(Collision2D collision)
    //  {
    //     if (collision.gameObject.CompareTag("Ground"))
    //     {
    //          isOnGround = true;
    //      }
    // }

    //   private void OnCollisionExit2D(Collision2D collision)
    //    {
    //       if (collision.gameObject.CompareTag("Ground"))
    //       {
    //           isOnGround = false;
    //      }
    //   }
}
