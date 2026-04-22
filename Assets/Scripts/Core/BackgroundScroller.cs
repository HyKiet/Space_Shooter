using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 1.25f;
    [SerializeField] private float tileHeight = 20f;
    [SerializeField] private Transform resetTarget;

    private void Update()
    {
        transform.position += Vector3.down * (scrollSpeed * Time.deltaTime);

        if (resetTarget == null)
        {
            return;
        }

        if (transform.position.y <= resetTarget.position.y - tileHeight)
        {
            transform.position = new Vector3(
                transform.position.x,
                resetTarget.position.y + tileHeight,
                transform.position.z);
        }
    }
}
