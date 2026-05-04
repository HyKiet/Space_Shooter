using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Pause Menu - nhấn ESC để pause/unpause
/// Tích hợp với GameManager state, hỗ trợ Resume + Quit
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitToMenuButton;

    [Header("Audio")]
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;

    private bool isPaused = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Đảm bảo panel ẩn khi start
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Gán button events
        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);
        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
        if (quitToMenuButton != null)
            quitToMenuButton.onClick.AddListener(QuitToMenu);

        // Subscribe game state để tự ẩn khi game over
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void Update()
    {
        // Chỉ cho pause khi đang Playing
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            // Nếu game không ở trạng thái Playing, force unpause
            if (isPaused) ForceUnpause();
            return;
        }

        // Dùng New Input System (giống PlayerController.cs)
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        PlaySfx(pauseSound);
        Debug.Log("[PauseMenu] Game paused");
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        PlaySfx(resumeSound);
        Debug.Log("[PauseMenu] Game resumed");
    }

    private void ForceUnpause()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Restart()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.PlayAgain();
    }

    private void QuitToMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        // Tự động unpause khi game over hoặc về menu
        if (state != GameManager.GameState.Playing && isPaused)
            ForceUnpause();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, 0.6f);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }
}
