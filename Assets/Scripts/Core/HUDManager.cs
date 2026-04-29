using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD Manager - Health bar, Wave text, Kill counters (enemy + meteor)
/// Tự tìm Player Damageable lúc runtime
/// Dùng CanvasGroup alpha để ẩn/hiện thay vì setActive (tránh mất event subscription)
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthLabel;

    [Header("Wave")]
    [SerializeField] private Text waveText;

    [Header("Score")]
    [SerializeField] private Text scoreText;

    [Header("Kill Counters")]
    [SerializeField] private Text enemyKillText;
    [SerializeField] private Text meteorKillText;

    private Damageable playerDamageable;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Tự tìm Player Damageable lúc runtime
        PlayerController player = FindObjectOfType<PlayerController>(true);
        if (player != null)
        {
            playerDamageable = player.GetComponent<Damageable>();
            Debug.Log($"[HUDManager] Found player: {player.name}, Damageable={playerDamageable != null}");
        }

        if (playerDamageable != null)
        {
            playerDamageable.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerDamageable.CurrentHealth, playerDamageable.MaxHealth);
            Debug.Log($"[HUDManager] Subscribed to health events. HP={playerDamageable.CurrentHealth}/{playerDamageable.MaxHealth}");
        }
        else
        {
            Debug.LogWarning("[HUDManager] Player Damageable not found!");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled += UpdateEnemyKills;
            GameManager.Instance.OnMeteorKilled += UpdateMeteorKills;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        // Initial values
        UpdateEnemyKills(0);
        UpdateMeteorKills(0);
        UpdateWave(0);
        UpdateScore(0);

        if (GameManager.Instance != null)
        {
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthBarFill != null)
        {
            float fill = max > 0 ? current / max : 0f;
            healthBarFill.fillAmount = fill;
            Debug.Log($"[HUDManager] Health updated: {current}/{max} → fillAmount={fill}");
        }
    }

    private void UpdateEnemyKills(int kills)
    {
        if (enemyKillText != null)
            enemyKillText.text = kills + "x";
    }

    private void UpdateMeteorKills(int kills)
    {
        if (meteorKillText != null)
            meteorKillText.text = kills + "x";
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave: " + wave;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        // Dùng CanvasGroup để ẩn/hiện - KHÔNG dùng setActive (vì sẽ mất event subscription)
        bool show = (state == GameManager.GameState.Playing);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.interactable = show;
            canvasGroup.blocksRaycasts = show;
        }
    }

    private void OnDestroy()
    {
        if (playerDamageable != null)
            playerDamageable.OnHealthChanged -= UpdateHealth;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled -= UpdateEnemyKills;
            GameManager.Instance.OnMeteorKilled -= UpdateMeteorKills;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
