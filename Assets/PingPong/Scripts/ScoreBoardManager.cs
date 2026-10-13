using TMPro;
using UnityEngine;

public class ScoreBoardManager : MonoBehaviour
{
    public static ScoreBoardManager Instance;

    [Header("UI")]
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI enemyScoreText;
    public TextMeshProUGUI rallyText;

    private int playerScore = 0;
    private int enemyScore = 0;
    private int rallyCount = 0;

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddPlayerPoint()
    {
        playerScore++;
        rallyCount = 0;
        UpdateUI();
    }

    public void AddEnemyPoint()
    {
        enemyScore++;
        rallyCount = 0;
        UpdateUI();
    }

    public void IncrementRally()
    {
        rallyCount++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerScoreText.text = $"Player: {playerScore}";
        enemyScoreText.text = $"Enemy: {enemyScore}";
        rallyText.text = $"Rally: {rallyCount}";
    }
}
