using System;
using System.Collections;
using System.Linq.Expressions;
using Unity.Collections;
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

    [Header("Ground Snap")]
    [SerializeField] private float snapDistance = 0.08f;
    [SerializeField] private float snapUpBias = 0.02f;
    [SerializeField] private bool enableSnap = true;

    [Header("Debug")]
    [SerializeField] private bool drawGroundRay = true;

    private KinematicBody2D body;
    private GroundDetector2D ground;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private Vector2 velocity;
    private bool isFacingRight = true;

    private int jumpsRemaining;
    private bool isDashing;
    private bool wasGrounded;

    private float lastJumpPressedTime;
    private float gravity;

    private void Awake()
    {
        body = GetComponent<KinematicBody2D>();
        ground = GetComponent<GroundDetector2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (body == null) Debug.Log("KinematicBody2D is missing on the player!");
        if (ground == null) Debug.Log("GroundDetector2D is missing on the player");
        if (rb == null) Debug.Log("RigidBody2D missing on the player");

        gravity = Physics2D.gravity.y * 4f;

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


        if (Input.GetKeyUp(KeyCode.Space) && velocity.y > 0f)
        {                                                                                                       //variable jump height
           
            velocity.y *= jumpCutMultiplier;
           
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
        if (body == null || ground == null) return;

        bool groundedNow = ground.IsGrounded;


        if (!isDashing)
        {
            float targetX = moveInput.x * moveSpeed;

            float accel = groundedNow ? groundAcceleration : airAcceleration;
            float decel = groundedNow ? groundAcceleration : airDeceleration;

            if (Mathf.Abs(moveInput.x) > 0.01f)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, targetX, accel * Time.fixedDeltaTime);
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0f, decel * Time.fixedDeltaTime);
            }
        }
        if (!isDashing)
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }

        if (velocity.y < -maxFallSpeed)
            velocity.y = -maxFallSpeed;

        Vector2 delta = velocity * Time.fixedDeltaTime;
        body.Move(delta);

        if (enableSnap)
            SnapToGroundIfNeeded();
    }

    private void PerformJump()
    {
        float g = Mathf.Abs(gravity);
        float jumpVel = Mathf.Sqrt(2f * g * Mathf.Max(0.01f, desiredJumpHeight));

        jumpsRemaining = Mathf.Max(0, jumpsRemaining - 1);

        velocity.y = jumpVel;
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        float inputX = Input.GetAxisRaw("Horizontal");
        float xDir = inputX != 0 ? Mathf.Sign(inputX) : (isFacingRight ? 1f : -1f);

        velocity.x = xDir * dashSpeed;
        velocity.y = 0f;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
    }

    private void SnapToGroundIfNeeded()
    {
        if (!ground.IsGrounded) return;
        if (velocity.y > 0.05f) return;

        Collider2D col = body.col;
        if (col == null) return;

        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + snapUpBias);
        float rayLength = snapDistance + snapUpBias;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, ground.GroundMask);
        if (!hit) return;

        float offset = b.min.y - hit.point.y;

        if (offset > 0f && offset <= snapDistance)
        {
            body.rb.MovePosition(body.rb.position + Vector2.down * offset);

            if (velocity.y < 0f)
                velocity.y = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGroundRay) return;

        var kb = GetComponent<KinematicBody2D>();
        if (kb == null || kb.col == null) return;

        Bounds b = kb.col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y + snapUpBias);
        Vector2 endPoint = origin + Vector2.down * (snapDistance + snapUpBias);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, endPoint);
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
