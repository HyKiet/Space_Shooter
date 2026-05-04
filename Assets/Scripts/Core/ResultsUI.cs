using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Results UI - Màn hình kết quả đơn giản
/// FIX: Dùng CanvasGroup alpha thay vì SetActive(false) trong Awake
/// để Start() luôn được gọi và có thể subscribe GameManager.OnStateChanged
/// </summary>
public class ResultsUI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Text enemyKillText;
    [SerializeField] private Text meteorKillText;
    [SerializeField] private Text wavesSurvivedText;
    [SerializeField] private Text scoreText;         // Điểm hiện tại
    [SerializeField] private Text highScoreText;     // Kỷ lục cao nhất

    [Header("Buttons")]
    [SerializeField] private Button mainMenuButton;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // FIX: Dùng CanvasGroup để ẩn thay vì SetActive(false)
        // SetActive(false) trong Awake sẽ ngăn Start() chạy → mất event subscription
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetVisible(false);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha          = visible ? 1f : 0f;
        canvasGroup.interactable   = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Results)
        {
            SetVisible(true);
            UpdateStats();
        }
        else
        {
            SetVisible(false);
        }
    }

    private void UpdateStats()
    {
        if (GameManager.Instance == null) return;

        if (enemyKillText != null)
            enemyKillText.text = "x" + GameManager.Instance.EnemyKills;
        if (meteorKillText != null)
            meteorKillText.text = "x" + GameManager.Instance.MeteorKills;
        if (wavesSurvivedText != null)
            wavesSurvivedText.text = "Waves Survived: " + GameManager.Instance.CurrentWave;
        if (scoreText != null)
            scoreText.text = "Score: " + GameManager.Instance.Score;
        if (highScoreText != null)
            highScoreText.text = "Best: " + GameManager.Instance.HighscoreScore;
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
