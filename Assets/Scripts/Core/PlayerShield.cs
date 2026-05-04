using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerShield — Kích hoạt khiên bảo vệ có thời hạn cho player
///
/// Khi active:
///   - Gọi damageable.SetInvincible(true) → block toàn bộ damage
///   - Hiện sprite shield xoay xung quanh tàu
///   - Flash khi còn 2 giây sắp hết
///   - Tắt khi hết thời gian
///
/// Được gọi bởi ItemPickup.ApplyShield()
/// </summary>
public class PlayerShield : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float   defaultDuration   = 6f;    // Thời gian shield (giây)
    [SerializeField] private float   warningTime       = 2f;    // Flash cảnh báo khi còn N giây
    [SerializeField] private float   flashInterval     = 0.15f; // Tốc độ flash

    [Header("Visual")]
    [SerializeField] private SpriteRenderer shieldRenderer;     // Sprite khiên
    [SerializeField] private float rotateSpeed = 60f;           // Tốc độ xoay shield sprite

    [Header("Audio")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip deactivateSound;

    // ─── Private State ───
    private Damageable   damageable;
    private AudioSource  audioSrc;
    private bool         isActive   = false;
    private float        endTime;
    private Coroutine    flashRoutine;

    public bool IsActive => isActive;
    public float TimeRemaining => isActive ? Mathf.Max(0f, endTime - Time.time) : 0f;

    // ─── Events ───
    public event System.Action<float> OnShieldActivated;   // duration
    public event System.Action        OnShieldDeactivated;

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        audioSrc   = GetComponent<AudioSource>();
        if (audioSrc == null)
            audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;

        // Ẩn shield sprite ban đầu
        if (shieldRenderer != null)
            shieldRenderer.enabled = false;
    }

    private void Update()
    {
        if (!isActive) return;

        // Xoay shield sprite
        if (shieldRenderer != null && shieldRenderer.enabled)
            shieldRenderer.transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Kiểm tra hết hạn
        if (Time.time >= endTime)
            DeactivateShield();
    }

    // ─────────────────────────────────────────────────────
    /// <summary>Kích hoạt shield. Gọi từ ItemPickup.</summary>
    public void ActivateShield(float duration = -1f)
    {
        float dur = (duration > 0f) ? duration : defaultDuration;

        // Nếu đang active → renew (cộng thêm thời gian)
        if (isActive)
        {
            endTime = Mathf.Max(endTime, Time.time + dur);
            return;
        }

        isActive = true;
        endTime  = Time.time + dur;

        // Block damage
        if (damageable != null)
            damageable.SetInvincible(true);

        // Hiện visual
        if (shieldRenderer != null)
            shieldRenderer.enabled = true;

        // Âm thanh
        PlaySfx(activateSound);

        // Flash warning
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashWarningRoutine());

        OnShieldActivated?.Invoke(dur);
        Debug.Log($"[PlayerShield] Activated for {dur}s");
    }

    private void DeactivateShield()
    {
        if (!isActive) return;
        isActive = false;

        // Bỏ block damage
        if (damageable != null)
            damageable.SetInvincible(false);

        // Ẩn visual
        if (shieldRenderer != null)
        {
            shieldRenderer.enabled = false;
            shieldRenderer.color   = Color.white; // Reset màu
        }

        PlaySfx(deactivateSound);
        OnShieldDeactivated?.Invoke();
        Debug.Log("[PlayerShield] Deactivated");
    }

    // ─── Flash warning khi sắp hết ───
    private IEnumerator FlashWarningRoutine()
    {
        // Chờ đến khi còn warningTime giây
        float waitUntil = endTime - warningTime;
        while (Time.time < waitUntil && isActive)
            yield return null;

        if (!isActive) yield break;

        // Flash nhanh cho đến khi hết hạn
        bool visible = true;
        while (isActive)
        {
            if (shieldRenderer != null)
                shieldRenderer.color = visible
                    ? new Color(1f, 0.5f, 0.2f, 0.9f)   // Cam = cảnh báo
                    : new Color(1f, 1f, 1f, 0.3f);        // Mờ

            visible = !visible;
            yield return new WaitForSeconds(flashInterval);
        }

        flashRoutine = null;
    }

    // ─── Helper ───
    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && audioSrc != null)
            audioSrc.PlayOneShot(clip, 0.7f);
    }
}
