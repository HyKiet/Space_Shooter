using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD Manager - Health bar, Wave text, Kill counters (enemy + meteor)
/// Matches reference: Top-left Ship Health bar + Wave, Top-right kill icons
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Text healthLabel;

    [Header("Wave")]
    [SerializeField] private Text waveText;

    [Header("Kill Counters")]
    [SerializeField] private Text enemyKillText;
    [SerializeField] private Text meteorKillText;

    [Header("Player Reference")]
    [SerializeField] private Damageable playerDamageable;

    private void Start()
    {
        if (playerDamageable != null)
        {
            playerDamageable.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerDamageable.CurrentHealth, playerDamageable.MaxHealth);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled += UpdateEnemyKills;
            GameManager.Instance.OnMeteorKilled += UpdateMeteorKills;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        // Initial values
        UpdateEnemyKills(0);
        UpdateMeteorKills(0);
        UpdateWave(0);
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = max > 0 ? current / max : 0f;
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

    private void HandleStateChanged(GameManager.GameState state)
    {
        // Hide HUD when not playing
        gameObject.SetActive(state == GameManager.GameState.Playing || state == GameManager.GameState.GameOver);
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
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
