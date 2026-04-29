using UnityEngine;

/// <summary>
/// Projectile - bay theo direction, gây damage khi va chạm
/// Spawn explosion effect TẠI VỊ TRÍ TARGET (không phải vị trí đạn)
/// Dùng explode_2_0 prefab từ Resources
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private Vector2 direction = Vector2.up;
    [SerializeField] private bool isEnemyProjectile = false;

    private static GameObject _hitExplosionPrefab;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
    }

    public void SetAsEnemyProjectile()
    {
        isEnemyProjectile = true;
    }

    /// <summary>
    /// Spawn explosion effect TẠI VỊ TRÍ TARGET (trên enemy/player bị trúng)
    /// </summary>
    private void SpawnHitEffect(Vector3 targetPosition)
    {
        if (_hitExplosionPrefab == null)
            _hitExplosionPrefab = Resources.Load<GameObject>("Effects/Explode2");

        if (_hitExplosionPrefab != null)
        {
            var fx = Instantiate(_hitExplosionPrefab, targetPosition, Quaternion.identity);
            Destroy(fx, 0.5f); // Fallback - AutoDestroy sẽ tự hủy theo animation length
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isEnemyProjectile)
        {
            // Đạn enemy chỉ gây damage player
            if (!other.CompareTag("Player")) return;

            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                SpawnHitEffect(other.transform.position); // Nổ tại vị trí player
                Destroy(gameObject);
            }
        }
        else
        {
            // Đạn player - không damage player, chỉ damage enemy/meteor
            if (other.CompareTag("Player")) return;

            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                SpawnHitEffect(other.transform.position); // Nổ tại vị trí enemy/meteor
                Destroy(gameObject);
            }
        }
    }
}
