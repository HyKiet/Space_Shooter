using UnityEngine;

/// <summary>
/// Projectile - bay theo direction, gây damage khi va chạm.
/// Hỗ trợ 4 WeaponType:
///   Blaster  — Standard: destroy on hit
///   Laser    — Piercing: xuyên qua nhiều kẻ địch
///   Missile  — Homing:   tracking enemy gần nhất
///   Plasma   — AoE:      nổ vùng khi trúng, splash damage
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float speed    = 12f;
    [SerializeField] private float damage   = 25f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private Vector2 direction = Vector2.up;
    [SerializeField] private bool isEnemyProjectile = false;

    // ─── Laser — Piercing ───
    [Header("Laser (Piercing)")]
    [SerializeField] private bool isPiercing = false;
    [SerializeField] private int  maxPierceCount = 3;   // Xuyên tối đa 3 mục tiêu
    private int pierceCount = 0;

    // ─── Missile — Homing ───
    [Header("Missile (Homing)")]
    [SerializeField] private bool  isHoming      = false;
    [SerializeField] private float homingStrength = 180f;  // Độ xoay/giây (degrees)
    [SerializeField] private float homingRange    = 8f;    // Phạm vi tìm target
    [SerializeField] private float homingDelay    = 0.1f;  // Delay trước khi bắt đầu homing
    private Transform homingTarget;
    private float spawnTime;

    // ─── Plasma — AoE ───
    [Header("Plasma (AoE)")]
    [SerializeField] private bool  isAoE       = false;
    [SerializeField] private float aoeDamage   = 20f;
    [SerializeField] private float aoeRadius   = 1.0f;
    [SerializeField] private GameObject aoeVFXPrefab;

    // FIX: Dùng SerializedField thay vì Resources.Load
    // Resources.Load yêu cầu file phải nằm trong Assets/Resources/ → dễ bị silent fail
    [Header("Hit VFX")]
    [SerializeField] private GameObject hitVFXPrefab;

    // ─────────────────────────────────────────
    private void Start()
    {
        spawnTime = Time.time;
        Destroy(gameObject, lifetime);

        // Missile: xoay ngay về hướng bay
        if (isHoming)
            UpdateRotationToDirection();
    }

    private void Update()
    {
        if (isHoming && Time.time > spawnTime + homingDelay)
            HandleHoming();

        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);
    }

    // ─── Public Setup API ───
    public void SetDirection(Vector2 dir)
    {
        direction = dir;
        UpdateRotationToDirection();
    }

    public void SetAsEnemyProjectile() => isEnemyProjectile = true;

    public void SetPiercing(bool piercing, int maxPierce = 3)
    {
        isPiercing   = piercing;
        maxPierceCount = maxPierce;
    }

    public void SetHoming(bool homing, float strength = 180f, float range = 8f)
    {
        isHoming       = homing;
        homingStrength = strength;
        homingRange    = range;
    }

    public void SetAoE(bool aoe, float radius = 1.0f, float splashDmg = 20f)
    {
        isAoE     = aoe;
        aoeRadius = radius;
        aoeDamage = splashDmg;
    }

    // ─── Homing Logic ───
    private void HandleHoming()
    {
        // Tìm target mới nếu chưa có hoặc đã bị destroy
        if (homingTarget == null)
            homingTarget = FindNearestEnemy();

        if (homingTarget == null) return;

        // Xoay hướng về phía target
        Vector2 toTarget = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;
        direction = Vector2.MoveTowards(
            direction.normalized,
            toTarget,
            homingStrength * Time.deltaTime * Mathf.Deg2Rad
        ).normalized;

        UpdateRotationToDirection();
    }

    private Transform FindNearestEnemy()
    {
        // Dùng GetComponent thay vì CompareTag để tránh lỗi tag chưa đăng ký
        var hits = Physics2D.OverlapCircleAll(transform.position, homingRange);
        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            // Target hợp lệ: có Damageable, không phải Player, không phải pickup, không phải đạn
            if (hit.GetComponent<Damageable>() == null) continue;
            if (hit.CompareTag("Player")) continue;       // Bỏ qua player
            if (hit.GetComponent<ItemPickup>() != null) continue; // Bỏ qua pickup
            if (hit.GetComponent<Projectile>() != null) continue; // Bỏ qua đạn khác

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }

    private void UpdateRotationToDirection()
    {
        if (direction == Vector2.zero) return;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
    }

    // ─── Collision ───
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Bỏ qua trigger với chính nó
        if (other.gameObject == gameObject) return;

        // Bỏ qua pickup items (dùng GetComponent — không cần tag)
        if (other.GetComponent<ItemPickup>() != null) return;

        // Bỏ qua đạn khác (player hoặc enemy projectile)
        if (other.GetComponent<Projectile>() != null) return;

        if (isEnemyProjectile)
        {
            if (!other.CompareTag("Player")) return;

            var dmg = other.GetComponent<Damageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(damage);
                SpawnHitVFX(transform.position, 0.6f);
                Destroy(gameObject);
            }
        }
        else
        {
            // Đạn player → không damage player
            if (other.CompareTag("Player")) return;

            var dmg = other.GetComponent<Damageable>();
            if (dmg == null) return;

            // Gây damage chính
            dmg.TakeDamage(damage);
            SpawnHitVFX(transform.position, 0.5f);

            // AoE splash sau khi trúng
            if (isAoE)
                TriggerAoE(transform.position);

            if (isPiercing)
            {
                // Laser: xuyên qua, đếm pierce count
                pierceCount++;
                if (pierceCount >= maxPierceCount)
                    Destroy(gameObject);
                // Giảm damage mỗi lần xuyên
                damage *= 0.7f;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }


    // ─── AoE Splash ───
    private void TriggerAoE(Vector3 center)
    {
        var hits = Physics2D.OverlapCircleAll(center, aoeRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player")) continue; // Không damage chính mình

            var dmg = hit.GetComponent<Damageable>();
            if (dmg != null)
                dmg.TakeDamage(aoeDamage);
        }

        // VFX nổ AoE
        if (aoeVFXPrefab != null)
        {
            var fx = Instantiate(aoeVFXPrefab, center, Quaternion.identity);
            fx.transform.localScale = Vector3.one * (aoeRadius * 1.5f);
            Destroy(fx, 2f);
        }
    }

    // ─── Hit VFX ───
    private void SpawnHitVFX(Vector3 position, float scale = 1f)
    {
        // FIX: Dùng hitVFXPrefab inject qua Inspector, không dùng Resources.Load
        if (hitVFXPrefab == null) return;

        var fx = Instantiate(hitVFXPrefab, position, Quaternion.identity);
        if (scale != 1f)
            fx.transform.localScale = Vector3.one * scale;

        Destroy(fx, 2f);
    }

    // Gizmo để debug range homing + AoE
    private void OnDrawGizmosSelected()
    {
        if (isHoming)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, homingRange);
        }
        if (isAoE)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, aoeRadius);
        }
    }
}
