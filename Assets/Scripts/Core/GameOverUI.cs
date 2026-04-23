using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game Over UI - Panel với Current vs Highscore, Play Again + Main Menu buttons
/// Matches reference image 4
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Current Score")]
    [SerializeField] private Text currentEnemyKillText;
    [SerializeField] private Text currentMeteorKillText;
    [SerializeField] private Text currentWaveText;

    [Header("Highscore")]
    [SerializeField] private Text highscoreEnemyKillText;
    [SerializeField] private Text highscoreMeteorKillText;
    [SerializeField] private Text highscoreWaveText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
        {
            gameObject.SetActive(true);
            UpdateScores();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateScores()
    {
        if (GameManager.Instance == null) return;

        // Current
        if (currentEnemyKillText != null)
            currentEnemyKillText.text = "x" + GameManager.Instance.EnemyKills;
        if (currentMeteorKillText != null)
            currentMeteorKillText.text = "x" + GameManager.Instance.MeteorKills;
        if (currentWaveText != null)
            currentWaveText.text = "Wave: " + GameManager.Instance.CurrentWave;

        // Highscore
        if (highscoreEnemyKillText != null)
            highscoreEnemyKillText.text = "x" + GameManager.Instance.HighscoreEnemyKills;
        if (highscoreMeteorKillText != null)
            highscoreMeteorKillText.text = "x" + GameManager.Instance.HighscoreMeteorKills;
        if (highscoreWaveText != null)
            highscoreWaveText.text = "Wave: " + GameManager.Instance.HighscoreWave;
    }

    private void OnPlayAgain()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayAgain();
    }

    private void OnMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }
}
