using UnityEngine;

public class CoinState : MonoBehaviour
{
    [SerializeField] private bool collected;
    [SerializeField] private Vector2 startingPos;
    [SerializeField] private Collider2D coinCollider;
    [SerializeField] private SpriteRenderer coinSprite;

    private void Awake()
    {
        startingPos = transform.position;
        collected = false;
    }
    public void Collect()
    {
        collected = true;
        coinSprite.enabled = false;
        coinCollider.enabled = false;
    }

    public void ResetCoin()
    {
        collected = false;
        transform.position = startingPos;
        coinSprite.enabled = true;
        coinCollider.enabled = true;
    }
}
