using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KinematicBody2D : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float skinWidth = 0.02f;
    public Rigidbody2D rb { get; private set; }
    public Collider2D col { get; private set; }

    private ContactFilter2D contactFilter;
    private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[8];

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        contactFilter.useTriggers = false;
        contactFilter.useLayerMask = true;
        contactFilter.SetLayerMask(collisionMask);
    }

    public void Move(Vector2 delta)
    {
        Vector2 pos = rb.position;

        if (Mathf.Abs(delta.x) > 0.00001f)
        {
            float directionX = Mathf.Sign(delta.x);
            float distance = Mathf.Abs(delta.x) + skinWidth;

            int hitCount = rb.Cast(new Vector2(directionX, 0f), contactFilter, hitBuffer, distance);

            if (hitCount > 0)
            {
                float minDist = distance;
                for (int i = 0; i < hitCount; i++)
                {
                    if (hitBuffer[i].distance < minDist)
                        minDist = hitBuffer[i].distance;
                }
                distance = Mathf.Max(0f, minDist - skinWidth);
            }
            else
            {
                distance = Mathf.Abs(delta.x);
            }

            pos.x += distance * directionX;
        }

        if (Mathf.Abs(delta.y) > 0.00001f)
        {
            float directionY = Mathf.Sign(delta.y);
            float distance = Mathf.Abs(delta.y) + skinWidth;

            int hitCount = rb.Cast(new Vector2(0f, directionY), contactFilter, hitBuffer, distance);

            if (hitCount > 0)
            {
                float minDist = distance;
                for (int i = 0; i < hitCount; i++)
                {
                    if (hitBuffer[i].distance < minDist) minDist = hitBuffer[i].distance;
                }
                distance = Mathf.Max(0f, minDist - skinWidth);
            }
            else
            {
                distance = Mathf.Abs(delta.y);
            }
            
            pos.y += distance * directionY;
        }
        rb.MovePosition(pos);
    }

}


