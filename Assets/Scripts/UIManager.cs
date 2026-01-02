using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] public TextMeshProUGUI coinText;

    private void Awake()
    {
        winText.gameObject.SetActive(false);
    }
    public void UpdateCoinDisplay(int coins)
    {
        coinText.text = "Coins: "+ coins;
    }

    public void UpdateTimerDisplay(float elapsedTime)
    {
        String Timer = elapsedTime.ToString("0.00");
        text.text = Timer;

    }

    public void ShowWinText(float finalTime)
    {
        string finalTimeString = finalTime.ToString("0.00");
        winText.text = "You Win!\nTime: " + finalTimeString + "PRESS R TO RESTART";
        winText.gameObject.SetActive(true);
    }

    public void HideWinText()
    {
        winText.gameObject.SetActive(false);
    }
}
