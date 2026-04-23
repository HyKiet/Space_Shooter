using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Results UI - Màn hình kết quả đơn giản
/// Matches reference image 5: enemy kills, meteor kills, waves survived, Main Menu button
/// </summary>
public class ResultsUI : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private Text enemyKillText;
    [SerializeField] private Text meteorKillText;
    [SerializeField] private Text wavesSurvivedText;

    [Header("Buttons")]
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

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Results)
        {
            gameObject.SetActive(true);
            UpdateStats();
        }
        else
        {
            gameObject.SetActive(false);
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
