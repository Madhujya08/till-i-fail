using System.Net.NetworkInformation;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Run")]
    [SerializeField] private float moveSpeed = 7f;

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

    [Header("Fall")]
    [SerializeField] private float fallDeathY;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private EnemyState enemyState;

    public float moveHorizontal;
    private float targetSpeedX;

    public Rigidbody2D rb { get; private set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2.7f;
    }

    private void Update()
    {
        if (GameManager.Instance.levelCompleted == true) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
           wantsToJump = true;
        }

    }

    private void FixedUpdate()
    {
        float currentPositionY = rb.transform.position.y;

        Vector2 originCenter = groundCheckCenter.position;
        Vector2 originLeft = groundCheckLeft.position;
        Vector2 originRight = groundCheckRight.position;

        bool jumpPressedThisStep = wantsToJump;
        wantsToJump = false;
            
        targetSpeedX = moveHorizontal * moveSpeed;                                    
        Vector2 currentVelocity = rb.linearVelocity;                                    //example of read

        currentVelocity = new Vector2(targetSpeedX, currentVelocity.y);
        rb.linearVelocity = currentVelocity;                                           // example of write

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
        gameManager.elapsedTime = 0;
        EnemyState[] enemies = FindObjectsByType<EnemyState>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.ResetEnemy();
        }
    }
    
}
