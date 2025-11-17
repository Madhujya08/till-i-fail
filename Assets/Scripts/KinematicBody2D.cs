using Unity.VisualScripting;
using UnityEngine;

public class KinematicBody2D : MonoBehaviour
{
    public Rigidbody2D rb { get; private set; }
    public Collider2D col { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }
    public Vector2 Velocity
    {
        get => rb.linearVelocity;
        set => rb.linearVelocity = value;
    }

    public Vector2 Position
    {
        get => rb.position;
        set => rb.position = value;
    }
}


