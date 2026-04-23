using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Singleton quản lý game state: Menu → Playing → GameOver → Results
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, GameOver, Results }

    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Playing;
    public GameState CurrentState => currentState;

    // Events
    public event Action<GameState> OnStateChanged;
    public event Action<int> OnEnemyKilled;
    public event Action<int> OnMeteorKilled;
    public event Action<int> OnWaveChanged;
    public event Action OnPlayerDied;

    // Stats
    public int EnemyKills { get; private set; }
    public int MeteorKills { get; private set; }
    public int CurrentWave { get; private set; }
    public int HighscoreEnemyKills { get; private set; }
    public int HighscoreMeteorKills { get; private set; }
    public int HighscoreWave { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadHighscore();
    }

    private void Start()
    {
        SetState(GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                SaveHighscore();
                break;
            case GameState.Results:
                Time.timeScale = 0f;
                SaveHighscore();
                break;
        }
    }

    public void AddEnemyKill()
    {
        EnemyKills++;
        OnEnemyKilled?.Invoke(EnemyKills);
    }

    public void AddMeteorKill()
    {
        MeteorKills++;
        OnMeteorKilled?.Invoke(MeteorKills);
    }

    public void SetWave(int wave)
    {
        CurrentWave = wave;
        OnWaveChanged?.Invoke(wave);
    }

    public void PlayerDied()
    {
        OnPlayerDied?.Invoke();
        SetState(GameState.GameOver);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // Reload same scene for now (no main menu scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowResults()
    {
        SetState(GameState.Results);
    }

    private void SaveHighscore()
    {
        if (EnemyKills > HighscoreEnemyKills)
        {
            HighscoreEnemyKills = EnemyKills;
            PlayerPrefs.SetInt("HighscoreEnemyKills", HighscoreEnemyKills);
        }
        if (MeteorKills > HighscoreMeteorKills)
        {
            HighscoreMeteorKills = MeteorKills;
            PlayerPrefs.SetInt("HighscoreMeteorKills", HighscoreMeteorKills);
        }
        if (CurrentWave > HighscoreWave)
        {
            HighscoreWave = CurrentWave;
            PlayerPrefs.SetInt("HighscoreWave", HighscoreWave);
        }
        PlayerPrefs.Save();
    }

    private void LoadHighscore()
    {
        HighscoreEnemyKills = PlayerPrefs.GetInt("HighscoreEnemyKills", 0);
        HighscoreMeteorKills = PlayerPrefs.GetInt("HighscoreMeteorKills", 0);
        HighscoreWave = PlayerPrefs.GetInt("HighscoreWave", 0);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
