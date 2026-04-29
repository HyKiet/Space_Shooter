using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game Over UI - Panel với Current vs Highscore, Play Again + Main Menu buttons
/// Dùng CanvasGroup alpha để ẩn/hiện (giữ event subscription hoạt động)
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Current Score")]
    [SerializeField] private Text currentEnemyKillText;
    [SerializeField] private Text currentMeteorKillText;
    [SerializeField] private Text currentWaveText;
    [SerializeField] private Text currentScoreText;

    [Header("Highscore")]
    [SerializeField] private Text highscoreEnemyKillText;
    [SerializeField] private Text highscoreMeteorKillText;
    [SerializeField] private Text highscoreWaveText;
    [SerializeField] private Text highscoreScoreText;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Wire buttons
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(OnPlayAgain);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);

        // Subscribe to events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
        else
        {
            // Hide initially
            SetVisible(false);
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
        {
            SetVisible(true);
            UpdateScores();
        }
        else
        {
            SetVisible(false);
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
        if (currentScoreText != null)
            currentScoreText.text = "Score: " + GameManager.Instance.Score;

        // Highscore
        if (highscoreEnemyKillText != null)
            highscoreEnemyKillText.text = "x" + GameManager.Instance.HighscoreEnemyKills;
        if (highscoreMeteorKillText != null)
            highscoreMeteorKillText.text = "x" + GameManager.Instance.HighscoreMeteorKills;
        if (highscoreWaveText != null)
            highscoreWaveText.text = "Wave: " + GameManager.Instance.HighscoreWave;
        if (highscoreScoreText != null)
            highscoreScoreText.text = "Score: " + GameManager.Instance.HighscoreScore;
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
