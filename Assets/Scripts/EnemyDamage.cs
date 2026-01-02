using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private EnemyState enemyState;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Movement movement = collision.GetComponent<Movement>();
        if (movement != null)
        {
            if (enemyState.isDead == true)
            {
                return;
            }
            else
            {
                GameManager.Instance.RespawnPlayer();
            }
            
        }
        else
        {
            return;
        }
    }
}
