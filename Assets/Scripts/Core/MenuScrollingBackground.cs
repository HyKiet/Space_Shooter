using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Làm cho texture của RawImage cuộn tròn vô tận.
/// Thích hợp cho hiệu ứng mây hoặc sao di chuyển chậm ở nền Menu.
/// </summary>
public class MenuScrollingBackground : MonoBehaviour
{
    [Header("Components")]
    public RawImage rawImage;

    [Header("Settings")]
    public Vector2 scrollSpeed = new Vector2(0.05f, 0.02f);

    private Rect uvRect;

    private void Start()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();
        
        if (rawImage != null)
            uvRect = rawImage.uvRect;
    }

    private void Update()
    {
        if (rawImage == null) return;

        // Cập nhật tọa độ UV để tạo hiệu ứng cuộn
        uvRect.x += scrollSpeed.x * Time.deltaTime;
        uvRect.y += scrollSpeed.y * Time.deltaTime;
        
        rawImage.uvRect = uvRect;
    }
}
