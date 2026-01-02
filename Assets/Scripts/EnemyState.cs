using Unity.VisualScripting;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyPatrol enemyPatrol;

    [SerializeField] private Collider2D hurtBoxCollider;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private SpriteRenderer enemySpriteRenderer;

    [SerializeField] public bool isDead;

    [SerializeField] private Vector2 startingPos;
    [SerializeField] private int startingDir;


    private Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPos = rb.position;
        startingDir = enemyPatrol.direction;
    }

    public void Die()
    {
        isDead = true;
        hurtBoxCollider.enabled = false;
        enemyCollider.enabled = false;
        enemySpriteRenderer.enabled = false;
    }
    public void ResetEnemy()
    {
        isDead = false;
        hurtBoxCollider.enabled = true;
        enemyCollider.enabled = true;
        enemySpriteRenderer.enabled = true;
        rb.position = startingPos;
        enemyPatrol.direction = startingDir;
        Vector2 velocity = rb.linearVelocity;
        velocity.x = 0;
        velocity.y = 0;
        rb.linearVelocity = velocity;
        
    }

}
