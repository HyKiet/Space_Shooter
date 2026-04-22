using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private string[] hittableTags = { "Enemy", "Meteor" };

    private Vector2 direction = Vector2.up;
    private GameObject owner;

    public void Initialize(Vector2 moveDirection, float newSpeed, float newLifetime, float newDamage, GameObject projectileOwner)
    {
        direction = moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : Vector2.up;
        speed = newSpeed;
        lifeTime = newLifetime;
        damage = newDamage;
        owner = projectileOwner;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHit(collision.gameObject);
    }

    private void TryHit(GameObject other)
    {
        if (other == null || other == owner)
        {
            return;
        }

        var damageable = other.GetComponent<Damageable>() ?? other.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            damageable.ApplyDamage(damage);
            Destroy(gameObject);
            return;
        }

        foreach (string tagName in hittableTags)
        {
            if (!string.IsNullOrEmpty(tagName) && other.CompareTag(tagName))
            {
                Destroy(other);
                Destroy(gameObject);
                return;
            }
        }
    }
}
