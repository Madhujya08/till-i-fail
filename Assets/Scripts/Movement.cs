using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Movement : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private bool isMovementOverridden;

    [Header("Ground Check")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private Transform groundCheckCenter;
    [SerializeField] private Transform groundCheckLeft;
    [SerializeField] private Transform groundCheckRight;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool wantsToJump;
    [SerializeField] private int maxJumps = 2;
    [SerializeField] private int jumpsRemaining;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown;
    [SerializeField] private int maxDashCharges = 1;
    [SerializeField] private int dashChargesRemaining;
    [SerializeField] private bool isDashing;
    [SerializeField] private bool wantsToDash;
    private Vector2 dashAim;
    private Vector2 dashAimNormalized;

    [Header("Fall")]
    [SerializeField] private float fallDeathY;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnemyState enemyState;

    [SerializeField] private float overrideTimer;
    [SerializeField] private Vector2 overrideVelocity;

    public float moveHorizontal;
    public float moveVertical;
    private float targetSpeedX;
    private int facingDir = 1;
    private Vector2 desiredDirection;
    private TrailRenderer trailRenderer;
    public Rigidbody2D rb { get; private set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        rb.gravityScale = 2.7f;
        
    }

    private void Update()
    {
        if (GameManager.Instance.levelCompleted == true) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");
        dashAim = new Vector2(moveHorizontal, moveVertical);

        if (moveHorizontal > 0)
        {
            facingDir = 1;
        }
        else if (moveHorizontal < 0)
        {
            facingDir = -1;
        }

        // Natsu was here
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            wantsToDash = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            wantsToJump = true;
        }

    }

    private void FixedUpdate()
    {
        Vector2 currentVelocity = rb.linearVelocity;
        float currentPositionY = rb.transform.position.y;
        bool jumpPressedThisStep = wantsToJump;
        bool dashPressedThisStep = wantsToDash;
        wantsToDash = false;
        wantsToJump = false;
        targetSpeedX = moveHorizontal * moveSpeed;

        Vector2 originCenter = groundCheckCenter.position;
        Vector2 originLeft = groundCheckLeft.position;
        Vector2 originRight = groundCheckRight.position;


        isGrounded = false;
        RaycastHit2D hitCenter = Physics2D.Raycast(originCenter, Vector2.down, groundCheckDistance, groundLayerMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.down, groundCheckDistance, groundLayerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.down, groundCheckDistance, groundLayerMask);

        if(hitCenter.collider != null)
            isGrounded = true;
        if (hitLeft.collider != null)
            isGrounded = true;
        if (hitRight.collider != null)
            isGrounded = true;

        if (overrideTimer > 0)
        {
            overrideTimer -= Time.fixedDeltaTime;
            if (overrideTimer < 0)
            {
                overrideTimer = 0;
            }

            
            rb.linearVelocity = overrideVelocity;
            return;
        }
        if (isGrounded == true)
        {
            dashChargesRemaining = maxDashCharges;
        }

        if (dashPressedThisStep && dashChargesRemaining > 0)
        {
            startDash();
            dashChargesRemaining--;
            return;
        }
        else
        {
            trailRenderer.emitting = false;
        }

        currentVelocity = new Vector2(targetSpeedX, currentVelocity.y);
        rb.linearVelocity = currentVelocity;

        if (isGrounded == true && currentVelocity.y <= 0)
            jumpsRemaining = maxJumps;


        if (jumpPressedThisStep && jumpsRemaining > 0)
            {
                currentVelocity = new Vector2(currentVelocity.x, jumpForce);
                rb.linearVelocity = currentVelocity;
                jumpsRemaining -= 1;
                wantsToJump = false;
            }

        
        if (currentPositionY < fallDeathY)
            GameManager.Instance.RespawnPlayer();

    }

    public void RespawnAt(Transform spawnPoint)
    {
        rb.position = spawnPoint.position;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = 0;
        velocity.y = 0;
        rb.linearVelocity = velocity;
        jumpsRemaining = maxJumps;
        moveHorizontal = 0f;
        wantsToJump = false;
        dashChargesRemaining = maxDashCharges;
        gameManager.elapsedTime = 0;
        EnemyState[] enemies = FindObjectsByType<EnemyState>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.ResetEnemy();
        }
    }

    private void startDash()
    {
        Vector2 aimDir = dashAim;

        if (dashAim.sqrMagnitude <= 0.0001f)
        {
            aimDir = new Vector2(facingDir, 0f);
        }

        Vector2 snappedDir = SnapTo8Direction(aimDir);

        overrideVelocity = snappedDir * dashSpeed;
        overrideTimer = dashDuration;

        rb.linearVelocity = overrideVelocity;
        trailRenderer.emitting = true;
    }

    private static Vector2 SnapTo8Direction(Vector2 dir)
    {
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float snappedDeg = Mathf.Round(angleDeg / 45f) * 45f;
        float rad = snappedDeg * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }
}
