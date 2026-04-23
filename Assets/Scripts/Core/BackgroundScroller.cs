using UnityEngine;

/// <summary>
/// Background vô hạn scrolling xuống dưới
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private float resetHeight = 20f;
    [SerializeField] private float startY = 0f;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        // Reset khi xuống quá xa
        if (transform.position.y <= -resetHeight)
        {
            Vector3 pos = transform.position;
            pos.y += resetHeight * 2f;
            transform.position = pos;
        }
    }
}
