using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Raycasts")]
    [SerializeField] private Transform groundProbeRight;
    [SerializeField] private Transform groundProbeLeft;
    [SerializeField] private Transform wallProbeRight;
    [SerializeField] private Transform wallProbeLeft;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;

    [Header("Enemy Movement")]
    [SerializeField] public float moveSpeed = 1f;
    [SerializeField] public int direction = 1;
 
    public Rigidbody2D rb;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        direction = 1;
        moveSpeed = 1;
    }
    private void FixedUpdate()
    {
        Vector2 groundPosition;
        Vector2 wallPosition;

        if (direction == 1)
        {
            groundPosition = groundProbeRight.position;
            wallPosition = wallProbeRight.position;
        }

        else
        {
            groundPosition = groundProbeLeft.position;
            wallPosition = wallProbeLeft.position;
        }

        Vector2 forward = new Vector2(direction, 0);

        RaycastHit2D groundAheadRay = Physics2D.Raycast(groundPosition, Vector2.down, groundCheckDistance, groundLayerMask);
        RaycastHit2D wallAheadRay = Physics2D.Raycast(wallPosition, forward, wallCheckDistance, groundLayerMask);

        if (groundAheadRay.collider == null || wallAheadRay.collider != null)
        {
            direction *= -1;

        }

        float targetXSpeed = moveSpeed * direction;
        Vector2 currentVelocity = rb.linearVelocity;
        currentVelocity = new Vector2(targetXSpeed, currentVelocity.y);
        rb.linearVelocity = currentVelocity;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 groundPositionStart;
        Vector2 wallPositionStart;

        if (direction == 1)
        {
            groundPositionStart = groundProbeRight.position;
            wallPositionStart = wallProbeRight.position;
        }
        else
        {
            groundPositionStart = groundProbeLeft.position;
            wallPositionStart = wallProbeLeft.position;
        }
        
        Vector2 down = Vector2.down;
        Vector2 groundPositonEnd = groundPositionStart + (down * groundCheckDistance);

        Vector2 forward = new Vector2(direction, 0);
        Vector2 wallPositionEnd = wallPositionStart + (forward * wallCheckDistance);

        Gizmos.DrawLine(groundPositionStart, groundPositonEnd);
        Gizmos.DrawLine(wallPositionStart, wallPositionEnd);
        Gizmos.color = Color.yellow;
    }
}
