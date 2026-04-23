using UnityEngine;

/// <summary>
/// World-space health bar cho enemy/meteor - tự tạo bằng SpriteRenderer
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private float barWidth = 0.8f;
    [SerializeField] private float barHeight = 0.1f;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private Color healthColor = Color.red;

    private Damageable damageable;
    private Transform barBackground;
    private Transform barFill;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        CreateHealthBar();
    }

    private void Start()
    {
        if (damageable != null)
        {
            damageable.OnHealthChanged += UpdateBar;
        }
    }

    private void CreateHealthBar()
    {
        // Background (black)
        var bgObj = new GameObject("HealthBarBG");
        bgObj.transform.SetParent(transform);
        bgObj.transform.localPosition = offset;
        bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        var bgSR = bgObj.AddComponent<SpriteRenderer>();
        bgSR.sprite = CreatePixelSprite();
        bgSR.color = backgroundColor;
        bgSR.sortingOrder = 10;
        barBackground = bgObj.transform;

        // Fill (red)
        var fillObj = new GameObject("HealthBarFill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = offset;
        fillObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);
        var fillSR = fillObj.AddComponent<SpriteRenderer>();
        fillSR.sprite = CreatePixelSprite();
        fillSR.color = healthColor;
        fillSR.sortingOrder = 11;
        barFill = fillObj.transform;
    }

    private void UpdateBar(float current, float max)
    {
        if (barFill == null) return;
        float percent = max > 0 ? current / max : 0f;
        Vector3 scale = barFill.localScale;
        scale.x = barWidth * percent;
        barFill.localScale = scale;

        // Offset fill to align left
        Vector3 pos = barFill.localPosition;
        pos.x = offset.x - (barWidth * (1f - percent)) / 2f;
        barFill.localPosition = pos;
    }

    private Sprite CreatePixelSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnHealthChanged -= UpdateBar;
    }
}
