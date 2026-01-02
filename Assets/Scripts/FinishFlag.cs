using UnityEngine;

public class FinishFlag : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Movement movement = collision.GetComponent<Movement>();

        if (movement == null)
            return;
        
        if (movement != null)
        {
            GameManager.Instance.OnLevelCompleted();
        }
    }
}
