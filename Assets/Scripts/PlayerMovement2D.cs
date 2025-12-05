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
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Fall")]
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.25f;
    [SerializeField] private bool dashInAirOnly = false;

    [Header("Debug")]
    [SerializeField] private bool drawGroundRay = true;

    private KinematicBody2D body;
    private GroundDetector2D ground;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private bool isFacingRight = true;

    private int jumpsRemaining;
    private bool isDashing;
    private bool wasGrounded;

    private float lastJumpPressedTime;

    private void Awake()
    {
        body = GetComponent<KinematicBody2D>();
        ground = GetComponent<GroundDetector2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (body == null) Debug.Log("KinematicBody2D is missing on the player!");
        if (ground == null) Debug.Log("GroundDetector2D is missing on the player");
        if (ground == null) Debug.Log("RigidBody2D missing on the player");

        if (rb != null)
        {
            rb.gravityScale = 4f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        //long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();


        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpPressedTime = jumpBufferTime;                                                //buffer jump input
        else
            lastJumpPressedTime -= Time.deltaTime;


        if (Input.GetKeyUp(KeyCode.Space) && body.Velocity.y > 0f)
        {                                                                                                       //variable jump height
            Vector2 v = body.Velocity;
            v.y *= jumpCutMultiplier;
            body.Velocity = v;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)&& !isDashing)
        {
            if (!dashInAirOnly || (dashInAirOnly && !ground.IsGrounded))                                                     //dash 
                StartCoroutine(Dash());
        }

        bool groundedNow = ground.IsGrounded;
        bool canCoyoteJump = !groundedNow && ground.TimeSinceLastGround <= coyoteTime;

        if (groundedNow && !wasGrounded)
        {
            jumpsRemaining = maxJumps;
        }
        wasGrounded = groundedNow;
       
        if (lastJumpPressedTime > 0f && jumpsRemaining > 0 && (groundedNow || jumpsRemaining < maxJumps || canCoyoteJump))
        {
            PerformJump();
            lastJumpPressedTime = 0f;
        }
    }

    private void FixedUpdate()
    {
        bool groundedNow = ground.IsGrounded;

        if (!isDashing)
        {
            float targetX = moveInput.x * moveSpeed;

            float accel = groundedNow ? groundAcceleration : airAcceleration;
            float decel = groundedNow ? groundAcceleration : airDeceleration;

            Vector2 v = body.Velocity;
            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                v.x = Mathf.MoveTowards(v.x, targetX, accel * Time.fixedDeltaTime);
            }
            else
            {
                v.x = Mathf.MoveTowards(v.x, 0f, decel * Time.fixedDeltaTime);
            }

            v.y = Mathf.Max(v.y, -maxFallSpeed);
            body.Velocity = v;
        }

        else
        {
            Vector2 v = body.Velocity;
            v.y = Mathf.Max(v.y, -maxFallSpeed);
            body.Velocity = v;
        }
    }

    private void PerformJump()
    {
        float g = -Physics2D.gravity.y * body.rb.gravityScale;
        float jumpVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, desiredJumpHeight));

        jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);

        Vector2 v = body.Velocity;
        v.y = jumpVel;
        body.Velocity = v;
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        float originalGravityScale = body.rb.gravityScale;
        body.rb.gravityScale= 0f;

        float inputX = Input.GetAxisRaw("Horizontal");
        float xDir = inputX != 0 ? Mathf.Sign(inputX) : (isFacingRight ? 1f : -1f);

        body.Velocity = new Vector2(xDir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        body.rb.gravityScale = originalGravityScale;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
    }

    //private bool isGrounded()
    //{
    //    Bounds bounds = col.bounds;
    //    Vector2 origin = bounds.center;
    //    float distance = bounds.extents.y + groundCheckExtra;
    //     return Physics2D.Raycast(origin, Vector2.down , distance,  groundMask);
    // }

    // private bool isGrounded2() 
    // { float extraHeight = 0.1f; 
    //   RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GetComponent<Collider2D>().bounds.extents.y + extraHeight, LayerMask.GetMask("Ground")); 
    //   return hit.col != null; 
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
