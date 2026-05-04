using UnityEngine;
using System.Collections;

/// <summary>
/// MusicManager — Singleton quản lý nhạc nền theo game state
///
/// State mapping:
///   Menu    → menuMusic (FrozenJam - calm)
///   Playing → gameMusic (RailJet - action)
///   Boss    → bossMusic (TowerDefense - intense)
///   GameOver/Results → nhạc game fade out
///
/// Tự động cross-fade mượt giữa các track.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;   // FrozenJam (calming menu)
    [SerializeField] private AudioClip gameMusic;   // RailJet (action gameplay)
    [SerializeField] private AudioClip bossMusic;   // TowerDefense (intense boss)

    [Header("Volume Settings")]
    [SerializeField] private float masterVolume  = 0.65f;
    [SerializeField] private float fadeDuration  = 1.5f;   // Cross-fade duration (giây)
    [SerializeField] private float bossVolume    = 0.8f;   // Boss music to hơn một chút

    // ─── Two AudioSources for cross-fading ───
    private AudioSource sourceA;
    private AudioSource sourceB;
    private bool        usingA = true;

    private AudioClip   currentClip;
    private Coroutine   fadeCoroutine;

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // KHÔNG dùng DontDestroyOnLoad — game chỉ có 1 scene
        // Reload scene sẽ tạo MusicManager mới hoàn toàn, tránh duplicate trên WebGL

        // Tạo 2 AudioSource để cross-fade
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        SetupSource(sourceA);
        SetupSource(sourceB);
    }

    private void SetupSource(AudioSource src)
    {
        src.loop        = true;
        src.playOnAwake = false;
        src.volume      = 0f;
        src.spatialBlend = 0f; // 2D sound
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged  += HandleStateChanged;
            GameManager.Instance.OnBossSpawned   += HandleBossSpawned;
            GameManager.Instance.OnBossDefeated  += HandleBossDefeated;

            // Play track phù hợp với state hiện tại
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    // ─── State Handlers ───
    private void HandleStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Menu:
                PlayMusic(menuMusic, masterVolume);
                break;

            case GameManager.GameState.Playing:
                // Chỉ chuyển sang gameMusic nếu không đang phát bossMusic
                if (currentClip != bossMusic)
                    PlayMusic(gameMusic, masterVolume);
                break;

            case GameManager.GameState.GameOver:
            case GameManager.GameState.Results:
                FadeOut();
                break;
        }
    }

    private void HandleBossSpawned()
    {
        PlayMusic(bossMusic, bossVolume);
    }

    private void HandleBossDefeated()
    {
        // Trở về nhạc game bình thường sau khi boss chết
        PlayMusic(gameMusic, masterVolume);
    }

    // ─── Core: Cross-fade ───
    public void PlayMusic(AudioClip clip, float targetVolume = -1f)
    {
        if (clip == null || clip == currentClip) return;

        float vol = targetVolume > 0f ? targetVolume : masterVolume;
        currentClip = clip;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossFadeRoutine(clip, vol));
    }

    private IEnumerator CrossFadeRoutine(AudioClip newClip, float targetVol)
    {
        // activeSource = đang phát (cần fade OUT)
        // nextSource   = sẽ phát nhạc mới (cần fade IN)
        AudioSource activeSource = usingA ? sourceA : sourceB;
        AudioSource nextSource   = usingA ? sourceB : sourceA;

        // Setup source mới
        nextSource.clip   = newClip;
        nextSource.volume = 0f;
        nextSource.Play();

        usingA = !usingA; // Swap: nextSource trở thành active

        float startVolOld = activeSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            nextSource.volume   = Mathf.Lerp(0f, targetVol, t);   // Fade IN
            activeSource.volume = Mathf.Lerp(startVolOld, 0f, t); // Fade OUT

            yield return null;
        }

        nextSource.volume   = targetVol;
        activeSource.volume = 0f;
        activeSource.Stop();

        fadeCoroutine = null;
    }

    public void FadeOut()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        AudioSource active = usingA ? sourceA : sourceB;
        float startVol = active.volume;
        float elapsed  = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled vì game pause khi GameOver
            active.volume = Mathf.Lerp(startVol, 0f, elapsed / fadeDuration);
            yield return null;
        }

        active.Stop();
        active.volume = 0f;
        currentClip = null;

        fadeCoroutine = null;
    }

    // ─── Public API ───
    public void SetMasterVolume(float vol)
    {
        masterVolume = Mathf.Clamp01(vol);
        AudioSource active = usingA ? sourceA : sourceB;
        active.volume = masterVolume;
    }

    // ─── Cleanup ───
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
            GameManager.Instance.OnBossSpawned  -= HandleBossSpawned;
            GameManager.Instance.OnBossDefeated -= HandleBossDefeated;
        }
    }
}
