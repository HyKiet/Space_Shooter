using UnityEngine;

/// <summary>
/// Enemy ship - di chuyển xuống, bắn đạn, có health bar, gây damage player khi va chạm
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float horizontalDrift = 0.5f;
    [SerializeField] private float driftFrequency = 1f;

    [Header("Combat")]
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float fireInterval = 2.5f;
    [SerializeField] private AudioClip shootSound;

    [Header("Score")]
    [SerializeField] private int scoreValue = 1;

    private Damageable damageable;
    private float driftOffset;
    private float spawnX;
    private float nextFireTime;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    private void Start()
    {
        spawnX = transform.position.x;
        driftOffset = Random.Range(0f, Mathf.PI * 2f);
        nextFireTime = Time.time + Random.Range(1f, fireInterval);

        if (damageable != null)
        {
            damageable.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Move down with slight horizontal drift
        float xDrift = Mathf.Sin((Time.time + driftOffset) * driftFrequency) * horizontalDrift;
        Vector3 moveDir = new Vector3(xDrift, -moveSpeed, 0f);
        transform.position += moveDir * Time.deltaTime;

        // Shooting
        HandleShooting();

        // Destroy if off screen (below)
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void HandleShooting()
    {
        if (projectilePrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireInterval;

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position + Vector3.down * 0.5f;
        var bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(Vector2.down);
            proj.SetAsEnemyProjectile();
        }

        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, 0.4f);
        }
    }

    private void HandleDeath()
    {
        // Spawn explosion
        if (explosionPrefab != null)
        {
            var explo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explo, 2f);
        }

        // Play sound
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 0.7f);
        }

        // Add kill to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemyKill();
        }

        Destroy(gameObject);
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
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDeath -= HandleDeath;
    }
}
