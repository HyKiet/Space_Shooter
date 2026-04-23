using UnityEngine;

/// <summary>
/// Projectile - bay lên trên, gây damage khi va chạm
/// </summary>
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private Vector2 direction = Vector2.up;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            // Don't damage player (projectile is from player)
            if (other.CompareTag("Player")) return;

            damageable.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
