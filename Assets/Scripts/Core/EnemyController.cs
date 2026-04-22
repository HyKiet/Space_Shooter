using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private float despawnY = -6.5f;

    private WaveManager waveManager;
    private bool killedByDamage;
    private Damageable damageable;

    public void Initialize(WaveManager owner, float speed)
    {
        waveManager = owner;
        moveSpeed = speed;
    }

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.Died += OnDied;
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
        {
            damageable.Died -= OnDied;
        }

        if (waveManager != null)
        {
            waveManager.NotifyEnemyRemoved(killedByDamage, scoreValue);
        }
    }

    private void Update()
    {
        transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
        if (transform.position.y < despawnY)
        {
            Destroy(gameObject);
        }
    }

    private void OnDied(Damageable _)
    {
        killedByDamage = true;
    }
}
