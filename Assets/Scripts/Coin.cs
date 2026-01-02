using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private CoinState state;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Movement movement = collision.GetComponent<Movement>();
        if (movement != null)
        {
            GameManager.Instance.AddCoin();
            state.Collect();
        }
        else
        {
            return;
        }
    }
}
