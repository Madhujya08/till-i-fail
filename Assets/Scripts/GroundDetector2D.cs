using UnityEditor.ShaderGraph.Internal;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(KinematicBody2D))]
public class GroundDetector2D : MonoBehaviour
{
    [Header("Ground Probe")]
    [SerializeField] float probeDistance = 0.2f;
    [SerializeField] float probeWidthShrink = 0.1f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] int groundedMinFrames = 2;

    public bool IsGrounded { get; private set; }
    public bool WasGroundedLastFrame {  get; private set; }
    public float TimeSinceLastGround { get; private set; } = 0f;
    public Vector2 GroundNormal { get; private set; } = Vector2.up;

    private Collider2D col;
    private int groundedGraceFrames;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        ManualUpdate(Time.fixedDeltaTime);
    }

    public void ManualUpdate(float dt)
    {
        WasGroundedLastFrame = IsGrounded;
        bool hitSomething = ProbeGround(out RaycastHit2D bestHit);

        if(hitSomething)
        {
            groundedGraceFrames = groundedMinFrames;
            IsGrounded = true;
            TimeSinceLastGround = 0f;
            GroundNormal = bestHit.normal;
        }
        else
        {
            if (groundedGraceFrames > 0)
            {
                groundedGraceFrames--;
                IsGrounded = true;
            }
            else
            {
                IsGrounded = false;
            }
            TimeSinceLastGround += dt;
        }
    }

    private bool ProbeGround(out RaycastHit2D bestHit)
    {
        Bounds b = col.bounds;

        float margin = b.size.x * probeWidthShrink * 0.5f;
        float leftX = b.min.x + margin;
        float rightX = b.max.x - margin;
        float centerX = b.center.x;
        float y = b.min.y;

        Vector2 originLeft = new Vector2(leftX, y);
        Vector2 originCenter = new Vector2(centerX, y);
        Vector2 originRight = new Vector2(rightX, y);

        RaycastHit2D hitL = Physics2D.Raycast(originLeft, Vector2.down, probeDistance, groundMask);
        RaycastHit2D hitC = Physics2D.Raycast(originCenter, Vector2.down, probeDistance, groundMask);
        RaycastHit2D hitR = Physics2D.Raycast(originRight, Vector2.down, probeDistance, groundMask);

        bestHit = default;
        float bestDist = float.MaxValue;
        bool any = false;

        if (hitL.collider != null && hitL.distance < bestDist) { bestHit = hitL; bestDist = hitL.distance; any = true; }
        if (hitC.collider != null && hitC.distance < bestDist) { bestHit = hitC; bestDist = hitC.distance; any = true; }
        if (hitR.collider != null && hitR.distance < bestDist) { bestHit = hitR; bestDist = hitR.distance; any = true; }
        return any;
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Bounds b = col.bounds;

        float margin = b.size.x * probeWidthShrink * 0.5f;
        float leftX = b.min.x + margin;
        float rightX = b.max.x - margin;
        float centerX = b.center.x;
        float y = b.min.y;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector2(leftX, y), new Vector2(leftX, y - probeDistance));
        Gizmos.DrawLine(new Vector2(centerX, y), new Vector2(centerX, y - probeDistance));
        Gizmos.DrawLine(new Vector2(rightX, y), new Vector2(rightX, y - probeDistance));
    }
}
