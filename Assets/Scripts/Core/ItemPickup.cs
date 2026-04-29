using UnityEngine;

/// <summary>
/// Item pickup - rơi từ meteor khi bị phá hủy
/// 2 loại: HealthPickup (hồi máu), WeaponUpgrade (nâng cấp vũ khí)
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public enum ItemType { Health, WeaponUpgrade }

    [Header("Item Settings")]
    [SerializeField] private ItemType itemType = ItemType.Health;
    [SerializeField] private float healAmount = 30f;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float lifetime = 8f;

    [Header("Visual")]
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private Vector3 startPos;
    private float spawnTime;

    private void Start()
    {
        startPos = transform.position;
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Fall down
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Bob effect (lên xuống nhẹ)
        float bob = Mathf.Sin((Time.time - spawnTime) * bobFrequency) * bobAmplitude;
        Vector3 pos = transform.position;
        pos.x += bob * Time.deltaTime;
        transform.position = pos;

        // Rotate
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        // Destroy if off screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (itemType)
        {
            case ItemType.Health:
                ApplyHealth(other.gameObject);
                break;
            case ItemType.WeaponUpgrade:
                ApplyWeaponUpgrade(other.gameObject);
                break;
        }

        // Play pickup sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.8f);
        }

        Destroy(gameObject);
    }

    private void ApplyHealth(GameObject player)
    {
        var damageable = player.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.Heal(healAmount);
            Debug.Log($"[ItemPickup] Player healed for {healAmount} HP");
        }
    }

    private void ApplyWeaponUpgrade(GameObject player)
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.UpgradeWeapon();
            Debug.Log("[ItemPickup] Player weapon upgraded!");
        }
    }
}
