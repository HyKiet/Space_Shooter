using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text killsText;
    [SerializeField] private Text waveText;

    public int Score { get; private set; }
    public int Kills { get; private set; }
    public int CurrentWave { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshUI();
    }

    public void ResetRun()
    {
        Score = 0;
        Kills = 0;
        CurrentWave = 0;
        RefreshUI();
    }

    public void SetWave(int wave)
    {
        CurrentWave = Mathf.Max(1, wave);
        RefreshUI();
    }

    public void RegisterEnemyKill(int scoreValue)
    {
        Kills++;
        Score += Mathf.Max(0, scoreValue);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Score;
        }

        if (killsText != null)
        {
            killsText.text = "Kills: " + Kills;
        }

        if (waveText != null)
        {
            waveText.text = "Wave: " + Mathf.Max(CurrentWave, 1);
        }
    }
}
