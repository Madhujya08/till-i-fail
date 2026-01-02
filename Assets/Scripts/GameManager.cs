using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Object Field Inspector")]
    [SerializeField] private Movement movement;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private EnemyState enemyState;
    [SerializeField] private CoinState coinState;

    [Header("Data")]
    [SerializeField] public float elapsedTime { get; set; }
    [SerializeField] private bool timerRunning;
    [SerializeField] private int coinsCollected;

    public bool levelCompleted;
    private bool canRestart;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;

        elapsedTime = 0;
        timerRunning = true;
        levelCompleted = false;
        canRestart = false;
    }

    private void Update()
    {
        float frameTime = Time.deltaTime;
        if (timerRunning == true)
        {
            elapsedTime += frameTime;
        }

        uiManager.UpdateTimerDisplay(elapsedTime);

        if (canRestart == true && Input.GetKeyDown(KeyCode.R))
        {
            RestartRun();
        }
    }
    public void AddCoin()
    {
        coinsCollected++;
        uiManager.UpdateCoinDisplay(coinsCollected);

    }
    public void RespawnPlayer()
    {
        movement.RespawnAt(spawnPoint);
    }

    public void OnLevelCompleted()
    {
        levelCompleted = true;
        timerRunning = false;

        uiManager.ShowWinText(elapsedTime);
        canRestart = true;
    }

    public void RestartRun()
    {
        levelCompleted = false;
        timerRunning = true;
        canRestart = false;
        elapsedTime = 0;
        uiManager.HideWinText();
        coinsCollected = 0;
        uiManager.UpdateCoinDisplay(0);
        CoinState[] coins = FindObjectsByType<CoinState>(FindObjectsSortMode.None);
        foreach (var coin in coins)
        {
            coin.ResetCoin();
        }

        EnemyState[] enemies = FindObjectsByType<EnemyState>(FindObjectsSortMode.None);
        foreach (var enemy in enemies) 
        {
            enemy.ResetEnemy();
        }
        RespawnPlayer();
    }
}
