using System;
using System.Collections;
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

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckExtra = 0.05f;

    [Header("Debug")]
    [SerializeField] private bool drawGroundRay = true;

    private Rigidbody2D rb;
    private Collider2D collider;
    private SpriteRenderer sr;
    private Sprite currentSprite;
    private Vector2 moveInput;
    private bool isFacingRight = true;

    private int jumpsRemaining;
    private bool isDashing;
    private bool wasGrounded;

    private long lastOnGroundTime;
    private float lastJumpPressedTime;
    private float baseGravity;

    public bool isOnGround;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        currentSprite = sr.sprite;

        rb.gravityScale = 4f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        jumpsRemaining = maxJumps;

        
    }

    private void Start()
    {
        rb.gravityScale = 4f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

    }

    private void Update()
    {
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        bool grounded = isGrounded2();

        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpPressedTime = jumpBufferTime;                                                //buffer jump input
        else
            lastJumpPressedTime -= Time.deltaTime;


        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0f)                                                  //variable jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        if (Input.GetKeyDown(KeyCode.LeftShift)&& !isDashing)
        {
            if (!dashInAirOnly || (dashInAirOnly && !isGrounded2()))                                                     //dash 
                StartCoroutine(Dash());
        }


        if (grounded) lastOnGroundTime = currentTime;                        //coyote time 
        

        if (grounded && !wasGrounded)                                       //recharging the jump
        {
            jumpsRemaining = maxJumps;
        }

        wasGrounded = grounded;

       
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
        if (!isDashing)
        {
            bool grounded = isGrounded2();
            float targetX = moveInput.x * moveSpeed;

            float accel = grounded ? groundAcceleration : airAcceleration;
            float decel = grounded ? groundAcceleration : airDeceleration;

            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, accel * Time.fixedDeltaTime);
            if (Mathf.Abs(moveInput.x) < 0.01f)
                newX = Mathf.MoveTowards(rb.linearVelocity.x, 0, decel * Time.fixedDeltaTime);

            Vector2 velocity = rb.linearVelocity;
            velocity.x = newX;
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
            rb.linearVelocity = velocity;
        }
    }

    private void PerformJump()
    {
        float g = -Physics2D.gravity.y * rb.gravityScale;
        float jumpVel = Mathf.Sqrt(2f * g * desiredJumpHeight);

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

        rb.linearVelocity = new Vector2(xDir * dashSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravityScale;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
    }

    //private bool isGrounded()
    //{
    //    Bounds bounds = collider.bounds;
    //    Vector2 origin = bounds.center;
    //    float distance = bounds.extents.y + groundCheckExtra;
   //     return Physics2D.Raycast(origin, Vector2.down , distance,  groundMask);
  // }

    private bool isGrounded2() 
    { float extraHeight = 0.1f; 
      RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, GetComponent<Collider2D>().bounds.extents.y + extraHeight, LayerMask.GetMask("Ground")); 
      return hit.collider != null; 
    }


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
