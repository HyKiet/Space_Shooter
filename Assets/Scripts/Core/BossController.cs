// Force recompile
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Boss C7 "Ưng Thép" - Controller
/// 3 Phase dựa trên % HP:
///   Phase 1 (100%-66%): Sweep ngang, bắn 3 đạn fan
///   Phase 2 (66%-33%): Lắc nhanh, bắn 5 đạn + spawn minion
///   Phase 3 (33%-0%):  Dive + rapid fire
/// </summary>
public class BossController : MonoBehaviour
{
    // ───────── Events ─────────
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDeath;
    public event Action<int> OnPhaseChanged;           // phase 1/2/3

    // ───────── Inspector ─────────
    [Header("Boss Stats")]
    [SerializeField] private float maxHealth = 1500f;
    [SerializeField] private int bossScoreValue = 3000;

    [Header("Movement")]
    [SerializeField] private float sweepSpeed = 2f;
    [SerializeField] private float sweepRange = 4f;       // ±X movement range
    [SerializeField] private float targetY = 3.5f;        // Y vị trí treo boss

    [Header("Shooting — Phase 1")]
    [SerializeField] private float p1FireInterval = 2f;
    [SerializeField] private int   p1BulletCount  = 3;
    [SerializeField] private float p1SpreadAngle  = 25f;

    [Header("Shooting — Phase 2")]
    [SerializeField] private float p2FireInterval = 1.3f;
    [SerializeField] private int   p2BulletCount  = 5;
    [SerializeField] private float p2SpreadAngle  = 40f;
    [SerializeField] private float p2MinionInterval = 8f;

    [Header("Shooting — Phase 3")]
    [SerializeField] private float p3FireInterval = 0.5f;
    [SerializeField] private int   p3BulletCount  = 7;
    [SerializeField] private bool  doDiveAttack   = true;
    [SerializeField] private float diveSpeed       = 8f;
    [SerializeField] private float diveInterval    = 5f;

    [Header("Prefabs")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject minionPrefab;   // C7 minion (colorC1)
    [SerializeField] private GameObject deathVFXPrefab; // Explosion VFX khi chết

    [Header("Shoot Points")]
    [SerializeField] private Transform[] shootPoints;   // Wing tips

    [Header("Drops on Death")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject weaponPickupPrefab;
    [SerializeField] private int healthDropCount  = 4;
    [SerializeField] private int weaponDropCount  = 2;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSfx;
    [SerializeField] private AudioClip phaseSfx;
    [SerializeField] private AudioClip deathSfx;
    [SerializeField] private AudioClip minionSfx;

    // ───────── Private state ─────────
    private float currentHealth;
    private int   currentPhase = 0;
    private bool  isDead       = false;
    private bool  isDiving     = false;

    private float nextFireTime;
    private float nextMinionTime;
    private float nextDiveTime;

    private float sweepDir = 1f;
    private Vector3 homePosition;
    // FIX: Phase 3 — luân phiên giữa Circle và Burst thay vì gọi cả 2 cùng lúc
    private bool p3AlternateAttack = false;

    private AudioSource audioSrc;

    // ───────── Lifecycle ─────────
    private void Awake()
    {
        currentHealth = maxHealth;
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        homePosition = new Vector3(0f, targetY, 0f);
        // Di chuyển boss từ phía trên xuống vị trí home
        StartCoroutine(EntranceRoutine());
    }

    private void Update()
    {
        if (isDead) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        HandleMovement();
        HandleShooting();
        HandlePhaseCheck();
    }

    // ───────── Entrance ─────────
    private IEnumerator EntranceRoutine()
    {
        // Start above screen
        transform.position = new Vector3(0f, 9f, 0f);
        float entrySpeed = 2f;

        while (transform.position.y > targetY + 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(0f, targetY, 0f),
                entrySpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = new Vector3(0f, targetY, 0f);
        homePosition = transform.position;

        // Bắt đầu chiến đấu
        EnterPhase(1);
    }

    // ───────── Movement ─────────
    private void HandleMovement()
    {
        float speed = currentPhase >= 2 ? sweepSpeed * 1.6f : sweepSpeed;
        transform.position += Vector3.right * sweepDir * speed * Time.deltaTime;

        if (transform.position.x > sweepRange)
        {
            sweepDir = -1f;
        }
        else if (transform.position.x < -sweepRange)
        {
            sweepDir = 1f;
        }
    }

    private float spiralAngle = 0f;

    // ───────── Shooting ─────────
    private void HandleShooting()
    {
        if (Time.time < nextFireTime) return;
        if (shootPoints == null || shootPoints.Length == 0) return;

        if (currentPhase == 1)
        {
            // Phase 1: Fan spread
            FireBurst(p1BulletCount, p1SpreadAngle);
            nextFireTime = Time.time + p1FireInterval;
        }
        else if (currentPhase == 2)
        {
            // Phase 2: Spiral attack liên tục
            FireSpiral();
            nextFireTime = Time.time + 0.15f; // Bắn siêu nhanh từng viên

            // Kèm thêm bắn quạt thỉnh thoảng
            if (UnityEngine.Random.value < 0.05f)
                FireBurst(p2BulletCount, p2SpreadAngle);

            // Phase 2: Spawn minion
            if (Time.time >= nextMinionTime)
            {
                SpawnMinions();
                nextMinionTime = Time.time + p2MinionInterval;
            }
        }
        else if (currentPhase == 3)
        {
            // FIX: Luân phiên Circle và Burst — trước đây gọi cả 2 = 23 đạn/0.5s → impossible
            // Giờ: 12 viên (circle) hoặc 7 viên (burst), cách nhau 0.8s → căng thẳng nhưng fair
            if (p3AlternateAttack)
                FireCircle(12);        // Pattern A: toả tròn 12 viên
            else
                FireBurst(p3BulletCount, 60f);  // Pattern B: quạt 7 viên

            p3AlternateAttack = !p3AlternateAttack;
            nextFireTime = Time.time + 0.8f;  // FIX: tăng từ 0.5s lên 0.8s
        }
    }

    private void FireBurst(int bulletCount, float totalSpread)
    {
        if (projectilePrefab == null) return;

        Transform[] points = (shootPoints != null && shootPoints.Length > 0)
            ? shootPoints
            : new Transform[] { transform };

        foreach (var sp in points)
        {
            for (int i = 0; i < bulletCount; i++)
            {
                float t = bulletCount > 1
                    ? (float)i / (bulletCount - 1) - 0.5f
                    : 0f;
                float angle = t * totalSpread;

                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.down;
                var bullet = Instantiate(projectilePrefab, sp.position, Quaternion.identity);
                var proj = bullet.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.SetDirection(dir);
                    proj.SetAsEnemyProjectile();
                }
            }
        }

        PlaySfx(shootSfx, 0.4f);
    }

    private void FireSpiral()
    {
        if (projectilePrefab == null) return;
        
        spiralAngle += 25f; // Xoay 25 độ mỗi lần bắn
        if (spiralAngle >= 360f) spiralAngle -= 360f;

        Vector2 dir = Quaternion.Euler(0, 0, spiralAngle) * Vector2.down;
        
        // Bắn từ tâm boss
        var bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(dir);
            proj.SetAsEnemyProjectile();
        }
        PlaySfx(shootSfx, 0.15f);
    }

    private void FireCircle(int count)
    {
        if (projectilePrefab == null) return;
        
        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, i * angleStep) * Vector2.down;
            var bullet = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            var proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetDirection(dir);
                proj.SetAsEnemyProjectile();
            }
        }
        PlaySfx(shootSfx, 0.5f);
    }

    // ───────── Minion Spawn ─────────
    private void SpawnMinions()
    {
        if (minionPrefab == null) return;

        Vector3[] spawnOffsets = new Vector3[]
        {
            transform.position + new Vector3(-2.5f, -0.5f, 0f),
            transform.position + new Vector3( 2.5f, -0.5f, 0f),
        };

        foreach (var pos in spawnOffsets)
        {
            Instantiate(minionPrefab, pos, Quaternion.identity);
        }

        PlaySfx(minionSfx, 0.6f);
    }

    // ───────── Phase Management ─────────
    private void HandlePhaseCheck()
    {
        float hpPercent = currentHealth / maxHealth;
        int targetPhase = hpPercent > 0.66f ? 1
                        : hpPercent > 0.33f ? 2
                        : 3;

        if (targetPhase != currentPhase)
            EnterPhase(targetPhase);
    }

    private void EnterPhase(int phase)
    {
        if (phase == currentPhase) return;
        currentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
        PlaySfx(phaseSfx, 0.8f);

        Debug.Log($"[BossController] Entering Phase {phase}");

        if (phase == 3)
        {
            nextDiveTime = Time.time + 2f;
            // Tint đỏ để báo hiệu phase cuối
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        }
    }

    // ───────── Damage ─────────
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Hit flash
        StartCoroutine(HitFlashRoutine());

        if (currentHealth <= 0f)
            Die();
    }

    private IEnumerator HitFlashRoutine()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        sr.color = original;
    }

    // ───────── Death ─────────
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        PlaySfx(deathSfx, 1f);

        // Death VFX sequence
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        // Nhiều vụ nổ nhỏ rải ra
        if (deathVFXPrefab != null)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3 offset = new Vector3(
                    UnityEngine.Random.Range(-1.5f, 1.5f),
                    UnityEngine.Random.Range(-1f, 1f), 0f);
                var fx = Instantiate(deathVFXPrefab, transform.position + offset, Quaternion.identity);
                fx.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.8f, 1.8f);
                Destroy(fx, 2f);
                yield return new WaitForSeconds(0.15f);
            }
        }

        // Drop nhiều item
        DropLoot();

        // Báo GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddBossKill(bossScoreValue);
        }

        OnDeath?.Invoke();
        Destroy(gameObject, 0.1f);
    }

    private void DropLoot()
    {
        // Drop health packs
        for (int i = 0; i < healthDropCount; i++)
        {
            if (healthPickupPrefab == null) break;
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-3f, 3f),
                UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
            Instantiate(healthPickupPrefab, transform.position + offset, Quaternion.identity);
        }

        // Drop weapon upgrades
        for (int i = 0; i < weaponDropCount; i++)
        {
            if (weaponPickupPrefab == null) break;
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-2f, 2f),
                UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
            Instantiate(weaponPickupPrefab, transform.position + offset, Quaternion.identity);
        }
    }

    // ───────── OnTriggerEnter ─────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Va chạm với player → damage player
        if (other.CompareTag("Player"))
        {
            var playerDmg = other.GetComponent<Damageable>();
            if (playerDmg != null)
                playerDmg.TakeDamage(30f);
        }
    }

    // ───────── Helper ─────────
    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null || audioSrc == null) return;
        audioSrc.PlayOneShot(clip, volume);
    }

    // ───────── Gizmos ─────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(0, targetY, 0),
            new Vector3(sweepRange * 2, 0.2f, 0));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
