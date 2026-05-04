using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Weapon HUD - Hiển thị vũ khí hiện tại + level ở góc dưới trái màn hình
/// Layout:
///   [Icon vũ khí 80x80]  [Tên vũ khí]
///   [● ○ ○] (Level dots)
///
/// Subscribe PlayerController.OnWeaponTypeChanged + OnWeaponLevelChanged
/// Hiệu ứng flash + bounce khi nhặt pickup
/// </summary>
public class WeaponHUDManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image   weaponIcon;        // Icon vũ khí hiện tại
    [SerializeField] private Text    weaponNameText;    // "BLASTER" / "LASER" / ...
    [SerializeField] private Image[] levelDots;         // 3 chấm cấp độ (Level 1/2/3)
    [SerializeField] private Image   panelBackground;   // Nền panel HUD

    [Header("Weapon Icons")]
    [SerializeField] private Sprite blasterIcon;
    [SerializeField] private Sprite laserIcon;
    [SerializeField] private Sprite missileIcon;
    [SerializeField] private Sprite plasmaIcon;

    [Header("Colors")]
    [SerializeField] private Color blasterColor  = new Color(0.4f, 0.8f, 1.0f);  // Xanh dương
    [SerializeField] private Color laserColor    = new Color(0.2f, 1.0f, 0.4f);  // Xanh lá
    [SerializeField] private Color missileColor  = new Color(1.0f, 0.6f, 0.1f);  // Cam
    [SerializeField] private Color plasmaColor   = new Color(0.8f, 0.2f, 1.0f);  // Tím
    [SerializeField] private Color dotActiveColor   = Color.white;
    [SerializeField] private Color dotInactiveColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Animation")]
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float bounceScale   = 1.25f;

    // ─── Private State ───
    private PlayerController player;
    private CanvasGroup      canvasGroup;
    private RectTransform    iconRect;
    private Coroutine        flashRoutine;

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (weaponIcon != null)
            iconRect = weaponIcon.GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Subscribe GameManager state để ẩn/hiện HUD
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }

        // Tìm Player và subscribe weapon events
        FindAndSubscribePlayer();
    }

    private void FindAndSubscribePlayer()
    {
        player = FindFirstObjectByType<PlayerController>(FindObjectsInactive.Include);
        if (player == null) return;

        player.OnWeaponTypeChanged  += HandleWeaponTypeChanged;
        player.OnWeaponLevelChanged += HandleWeaponLevelChanged;

        // Hiển thị trạng thái ban đầu
        HandleWeaponTypeChanged(player.CurrentWeapon);
        HandleWeaponLevelChanged(player.WeaponLevel);
    }

    // ─── Weapon Type Changed ───
    private void HandleWeaponTypeChanged(PlayerController.WeaponType type)
    {
        // Đổi icon
        if (weaponIcon != null)
        {
            Sprite icon = GetWeaponSprite(type);
            if (icon != null) weaponIcon.sprite = icon;
        }

        // Đổi tên
        if (weaponNameText != null)
            weaponNameText.text = GetWeaponName(type);

        // Đổi màu chủ đạo
        Color themeColor = GetWeaponColor(type);
        if (weaponNameText != null)
            weaponNameText.color = themeColor;
        ApplyDotTheme(themeColor);

        // Flash + bounce animation
        TriggerFlash();
    }

    // ─── Weapon Level Changed ───
    private void HandleWeaponLevelChanged(int level)
    {
        if (levelDots == null) return;

        for (int i = 0; i < levelDots.Length; i++)
        {
            if (levelDots[i] == null) continue;
            levelDots[i].color = (i < level) ? dotActiveColor : dotInactiveColor;
        }
    }

    // ─── Flash Animation ───
    private void TriggerFlash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (iconRect == null) yield break;

        Vector3 originalScale = Vector3.one;

        // Bounce up
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / flashDuration;
            float scale = Mathf.Lerp(1f, bounceScale, Mathf.Sin(t * Mathf.PI));
            iconRect.localScale = originalScale * scale;

            // Flash alpha của toàn panel
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(1f, 1.3f, Mathf.Sin(t * Mathf.PI));

            yield return null;
        }

        iconRect.localScale = originalScale;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        flashRoutine = null;
    }

    // ─── Helpers ───
    private Sprite GetWeaponSprite(PlayerController.WeaponType type)
    {
        switch (type)
        {
            case PlayerController.WeaponType.Laser:   return laserIcon   ?? blasterIcon;
            case PlayerController.WeaponType.Missile: return missileIcon ?? blasterIcon;
            case PlayerController.WeaponType.Plasma:  return plasmaIcon  ?? blasterIcon;
            default:                                   return blasterIcon;
        }
    }

    private string GetWeaponName(PlayerController.WeaponType type)
    {
        switch (type)
        {
            case PlayerController.WeaponType.Laser:   return "LASER";
            case PlayerController.WeaponType.Missile: return "MISSILE";
            case PlayerController.WeaponType.Plasma:  return "PLASMA";
            default:                                   return "BLASTER";
        }
    }

    private Color GetWeaponColor(PlayerController.WeaponType type)
    {
        switch (type)
        {
            case PlayerController.WeaponType.Laser:   return laserColor;
            case PlayerController.WeaponType.Missile: return missileColor;
            case PlayerController.WeaponType.Plasma:  return plasmaColor;
            default:                                   return blasterColor;
        }
    }

    private void ApplyDotTheme(Color color)
    {
        if (levelDots == null) return;
        // Chỉ đổi màu active dots sang theme color
        if (player == null) return;
        for (int i = 0; i < levelDots.Length; i++)
        {
            if (levelDots[i] == null) continue;
            if (i < player.WeaponLevel)
                levelDots[i].color = color;
            else
                levelDots[i].color = dotInactiveColor;
        }
    }

    // ─── Game State ───
    private void HandleStateChanged(GameManager.GameState state)
    {
        bool show = (state == GameManager.GameState.Playing);
        if (canvasGroup != null)
        {
            canvasGroup.alpha          = show ? 1f : 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    // ─── Cleanup ───
    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnWeaponTypeChanged  -= HandleWeaponTypeChanged;
            player.OnWeaponLevelChanged -= HandleWeaponLevelChanged;
        }
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }
}
