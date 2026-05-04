using UnityEngine;

/// <summary>
/// Item pickup - rơi từ enemy/meteor khi bị phá hủy
/// 5 loại: Health, WeaponUpgrade, LaserWeapon, MissileWeapon, PlasmaWeapon
/// </summary>
public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        Health,
        WeaponUpgrade,  // Tăng level đạn (spread)
        LaserWeapon,    // Đổi sang Laser — xuyên giáp
        MissileWeapon,  // Đổi sang Missile — homing
        PlasmaWeapon,   // Đổi sang Plasma — AoE
        Shield          // Khiên bảo vệ tạm thời
    }

    [Header("Item Settings")]
    [SerializeField] private ItemType itemType      = ItemType.Health;
    [SerializeField] private float    healAmount    = 30f;
    [SerializeField] private float    shieldDuration = 6f;  // Thời gian khiên (giây)
    [SerializeField] private float    fallSpeed     = 2f;
    [SerializeField] private float    rotateSpeed   = 90f;
    [SerializeField] private float    lifetime      = 8f;

    [Header("Visual")]
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobFrequency = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Rơi xuống
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // Bob effect (nhẹ nhàng)
        float bob = Mathf.Sin((Time.time - spawnTime) * bobFrequency) * bobAmplitude;
        Vector3 pos = transform.position;
        pos.x += bob * Time.deltaTime;
        transform.position = pos;

        // Xoay
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        if (transform.position.y < -7f)
            Destroy(gameObject);
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

            case ItemType.LaserWeapon:
                ApplyWeaponType(other.gameObject, PlayerController.WeaponType.Laser);
                break;

            case ItemType.MissileWeapon:
                ApplyWeaponType(other.gameObject, PlayerController.WeaponType.Missile);
                break;

            case ItemType.PlasmaWeapon:
                ApplyWeaponType(other.gameObject, PlayerController.WeaponType.Plasma);
                break;

            case ItemType.Shield:
                ApplyShield(other.gameObject);
                break;
        }

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.8f);

        Destroy(gameObject);
    }

    // ─── Apply Methods ───

    private void ApplyHealth(GameObject player)
    {
        var dmg = player.GetComponent<Damageable>();
        if (dmg != null)
        {
            dmg.Heal(healAmount);
            Debug.Log($"[ItemPickup] Healed {healAmount} HP");
        }
    }

    private void ApplyWeaponUpgrade(GameObject player)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.UpgradeWeapon();
            Debug.Log("[ItemPickup] Weapon level up!");
        }
    }

    private void ApplyWeaponType(GameObject player, PlayerController.WeaponType type)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetWeaponType(type);
            Debug.Log($"[ItemPickup] Weapon switched → {type}");
        }
    }

    private void ApplyShield(GameObject player)
    {
        var shield = player.GetComponent<PlayerShield>();
        if (shield != null)
        {
            shield.ActivateShield(shieldDuration);
            Debug.Log($"[ItemPickup] Shield activated for {shieldDuration}s");
        }
        else
        {
            Debug.LogWarning("[ItemPickup] PlayerShield component not found on player!");
        }
    }
}
