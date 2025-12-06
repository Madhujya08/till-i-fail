using UnityEditor.ShaderGraph.Internal;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(KinematicBody2D))]
public class GroundDetector2D : MonoBehaviour
{
    [Header("Ground Probe")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float probeHeight = 0.1f;
    [SerializeField] float probeShrink = 0.1f;


    public bool IsGrounded { get; private set; }
    public float TimeSinceLastGround { get; private set; } = 0f;

    private Collider2D col;

    public LayerMask GroundMask => groundMask;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (col == null) return;

        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * (1f - probeShrink), probeHeight);
        Vector2 center = new Vector2(b.center.x, b.min.y - probeHeight * 0.5f);

        bool hit = Physics2D.OverlapBox(center, size, 0f, groundMask);

        if (hit)
        {
            IsGrounded = true;
            TimeSinceLastGround = 0f;
        }
        else
        {
            IsGrounded = false;
            TimeSinceLastGround += Time.fixedDeltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (col == null) return;

        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x *(1f - probeShrink), probeHeight);
        Vector2 center = new Vector2(b.center.x, b.min.y - probeHeight * 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}
