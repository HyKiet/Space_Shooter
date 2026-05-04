using UnityEngine;

/// <summary>
/// Meteor - di chuyển xuống + xoay, gây damage player
/// Drop item khi bị phá hủy
/// </summary>
public class MeteorController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float horizontalSpeed = 0.3f;

    [Header("Combat")]
    [SerializeField] private float contactDamage = 30f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    [Header("Item Drop")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject weaponPickupPrefab;
    [SerializeField] private GameObject laserPickupPrefab;
    [SerializeField] private GameObject missilePickupPrefab;
    [SerializeField] private GameObject plasmaPickupPrefab;
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float healthDropRatio = 0.55f;  // 55% health
    [SerializeField] [Range(0f, 1f)] private float weaponDropRatio  = 0.25f; // 25% upgrade, 20% weapon type

    private Damageable damageable;
    private float hDir;
    // FIX: Guard chống double-kill — đặc biệt quan trọng với Laser (piercing)
    private bool isDead = false;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    private void Start()
    {
        hDir = Random.Range(-1f, 1f);
        rotationSpeed = Random.Range(30f, 100f) * (Random.value > 0.5f ? 1f : -1f);

        if (damageable != null)
        {
            damageable.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Move down with slight horizontal movement + rotate
        Vector3 move = new Vector3(hDir * horizontalSpeed, -moveSpeed, 0f);
        transform.position += move * Time.deltaTime;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // Destroy if off screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void HandleDeath()
    {
        // FIX: Chặn double-kill — Laser piercing có thể trigger HandleDeath nhiều lần
        if (isDead) return;
        isDead = true;

        if (explosionPrefab != null)
        {
            var explo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explo, 2f);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 0.7f);
        }

        // Drop item
        TryDropItem();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMeteorKill();
        }

        Destroy(gameObject);
    }

    private void TryDropItem()
    {
        if (Random.value > dropChance) return;

        float roll = Random.value;
        GameObject itemPrefab;

        if (roll < healthDropRatio)
        {
            // Health pickup
            itemPrefab = healthPickupPrefab;
        }
        else if (roll < healthDropRatio + weaponDropRatio)
        {
            // Weapon level upgrade
            itemPrefab = weaponPickupPrefab;
        }
        else
        {
            // Random weapon type pickup (Laser / Missile / Plasma)
            var weaponTypes = new GameObject[] { laserPickupPrefab, missilePickupPrefab, plasmaPickupPrefab };
            itemPrefab = weaponTypes[Random.Range(0, weaponTypes.Length)];
        }

        if (itemPrefab != null)
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerDamageable = other.GetComponent<Damageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(contactDamage);
            }
            // Meteor also takes damage from collision
            if (damageable != null)
            {
                damageable.TakeDamage(damageable.MaxHealth);
            }
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDeath -= HandleDeath;
    }
}
