using UnityEngine;

public class EnemyStompReciever : MonoBehaviour

{
    [SerializeField] private EnemyState enemyState;
    [SerializeField] private float bounceForce;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Movement movement = collision.GetComponent<Movement>();
        if (movement != null && movement.rb.linearVelocity.y < 0)
        {
            
            
                enemyState.Die();
                Vector2 currentVelocity = movement.rb.linearVelocity;
                currentVelocity = new Vector2(currentVelocity.x, bounceForce);
                movement.rb.linearVelocity = currentVelocity;
            
        }
        else
        {
            return;
        }
    }
}
