using UnityEngine;
using System.Collections;

/// <summary>
/// Camera Shake - rung camera khi player bị damage / enemy chết / boss bị damage
/// Gắn vào Main Camera. Gọi tĩnh: CameraShake.Instance.Shake(duration, magnitude)
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Default Shake Settings")]
    [SerializeField] private float defaultDuration = 0.18f;
    [SerializeField] private float defaultMagnitude = 0.12f;

    [Header("Preset Intensities")]
    [SerializeField] private float lightMagnitude  = 0.07f;  // Đạn trúng
    [SerializeField] private float mediumMagnitude = 0.14f;  // Player bị damage
    [SerializeField] private float heavyMagnitude  = 0.28f;  // Boss / nổ lớn

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        originalPos = transform.localPosition;
    }

    // ───────── Public API ─────────

    /// <summary>Rung camera với cường độ tùy chỉnh</summary>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration  < 0) duration  = defaultDuration;
        if (magnitude < 0) magnitude = defaultMagnitude;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    /// <summary>Rung nhẹ — đạn bắn trúng enemy</summary>
    public void ShakeLight()  => Shake(0.10f, lightMagnitude);

    /// <summary>Rung vừa — player bị damage</summary>
    public void ShakeMedium() => Shake(0.20f, mediumMagnitude);

    /// <summary>Rung mạnh — boss / nổ lớn / player chết</summary>
    public void ShakeHeavy()  => Shake(0.35f, heavyMagnitude);

    // ───────── Internal ─────────

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Magnitude giảm dần về cuối (easing out)
            float progress = elapsed / duration;
            float currentMagnitude = magnitude * (1f - progress);

            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.unscaledDeltaTime; // Dùng unscaled để hoạt động khi pause
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
