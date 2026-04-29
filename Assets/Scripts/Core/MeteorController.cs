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
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float healthDropRatio = 0.65f; // 65% health, 35% weapon

    private Damageable damageable;
    private float hDir;

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

        GameObject itemPrefab;
        if (Random.value < healthDropRatio)
        {
            itemPrefab = healthPickupPrefab;
        }
        else
        {
            itemPrefab = weaponPickupPrefab;
        }

        if (itemPrefab != null)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
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
