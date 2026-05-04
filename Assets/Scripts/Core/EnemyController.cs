using UnityEngine;

/// <summary>
/// Enemy ship AI v2 — Cải tiến:
///   1. OFF-SCREEN FIX:    Chỉ bắn khi enemy đã vào màn hình (Y dưới camera top)
///   2. WAVE SCALING:      moveSpeed + fireRate tăng theo wave number
///   3. AIM AT PLAYER:     Đạn hướng về phía player thay vì bắn thẳng xuống
///   4. DIFFICULTY TIERS:  Wave 1-4 = Easy, 5-9 = Medium, 10+ = Hard (thêm spread shot)
///
/// WaveSpawner gọi Init(waveNumber) ngay sau khi Instantiate.
/// </summary>
public class EnemyController : MonoBehaviour
{
    // ─── Inspector ───
    [Header("Movement")]
    [SerializeField] private float moveSpeed      = 2f;
    [SerializeField] private float horizontalDrift = 0.5f;
    [SerializeField] private float driftFrequency  = 1f;

    [Header("Combat")]
    [SerializeField] private float contactDamage   = 20f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip  explosionSound;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform  shootPoint;
    [SerializeField] private float      baseFireInterval = 2.5f; // Wave 1 interval
    [SerializeField] private AudioClip  shootSound;

    [Header("AI Settings")]
    [SerializeField] private float aimStrength     = 0.5f;   // 0 = thẳng xuống, 1 = full aim
    [SerializeField] private float screenEnterDelay = 0.3f;  // Thêm delay nhỏ sau khi vào screen
    [SerializeField] private float speedScalePerWave  = 0.08f; // +8% speed mỗi wave
    [SerializeField] private float fireScalePerWave   = 0.07f; // -7% interval mỗi wave
    [SerializeField] private float minFireInterval    = 0.8f;  // Bắn nhanh nhất tối đa

    // ─── Private State ───
    private Damageable   damageable;
    private float        driftOffset;
    private float        nextFireTime;
    private bool         isDead = false;

    // Wave-based parameters (được set bởi WaveSpawner qua Init)
    private int   waveNumber      = 1;
    private float scaledMoveSpeed;
    private float scaledFireInterval;

    // On-screen tracking
    private bool  hasEnteredScreen = false;  // FIX: chưa vào screen thì chưa bắn
    private float enteredScreenTime;
    private float cameraTopY;               // Y top của camera (tính 1 lần khi Start)

    // Hard mode (wave 10+): spread shot
    private bool isHardMode = false;

    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        damageable = GetComponent<Damageable>();
    }

    private void Start()
    {
        driftOffset = Random.Range(0f, Mathf.PI * 2f);

        // Tính Y top của camera để xác định enemy đã vào màn hình chưa
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 topWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));
            cameraTopY = topWorld.y + 0.5f; // Thêm buffer nhỏ
        }
        else
        {
            cameraTopY = 6f; // fallback
        }

        // Nếu Init chưa được gọi (prefab test trực tiếp), áp scaling wave 1
        if (scaledMoveSpeed <= 0f)
            ApplyWaveScaling(1);

        // Delay bắn ngẫu nhiên để enemy không bắn đồng loạt
        nextFireTime = Time.time + Random.Range(0.8f, scaledFireInterval);

        if (damageable != null)
            damageable.OnDeath += HandleDeath;
    }

    // ─────────────────────────────────────────────────────────
    /// <summary>
    /// Gọi bởi WaveSpawner ngay sau Instantiate để apply difficulty scaling
    /// </summary>
    public void Init(int wave)
    {
        waveNumber = Mathf.Max(1, wave);
        ApplyWaveScaling(waveNumber);
        isHardMode = (waveNumber >= 10);
    }

    private void ApplyWaveScaling(int wave)
    {
        // Speed tăng theo wave: wave 1 = base, wave 10 = +80%
        float speedMult = 1f + (wave - 1) * speedScalePerWave;
        speedMult = Mathf.Min(speedMult, 2.2f); // Cap tối đa 220%
        scaledMoveSpeed = moveSpeed * speedMult;

        // Fire interval giảm theo wave: wave 1 = base, giảm 7% mỗi wave
        float fireMult = 1f - (wave - 1) * fireScalePerWave;
        fireMult = Mathf.Max(fireMult, minFireInterval / baseFireInterval);
        scaledFireInterval = Mathf.Max(baseFireInterval * fireMult, minFireInterval);

        // Drift nhanh hơn ở wave cao
        float driftMult = 1f + (wave - 1) * 0.04f;
        horizontalDrift = Mathf.Min(horizontalDrift * driftMult, 1.8f);
    }

    // ─────────────────────────────────────────────────────────
    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();

        // FIX: Chỉ bắn khi đã vào màn hình
        CheckScreenEntry();
        if (hasEnteredScreen && Time.time >= enteredScreenTime + screenEnterDelay)
            HandleShooting();

        // Destroy nếu ra khỏi màn hình phía dưới
        if (transform.position.y < -7f)
            Destroy(gameObject);
    }

    // ─── Movement ───
    private void HandleMovement()
    {
        float xDrift = Mathf.Sin((Time.time + driftOffset) * driftFrequency) * horizontalDrift;
        Vector3 moveDir = new Vector3(xDrift, -scaledMoveSpeed, 0f);
        transform.position += moveDir * Time.deltaTime;
    }

    // ─── On-Screen Detection ───
    private void CheckScreenEntry()
    {
        if (hasEnteredScreen) return;

        // Enemy vào màn hình khi Y < cameraTopY
        if (transform.position.y < cameraTopY)
        {
            hasEnteredScreen  = true;
            enteredScreenTime = Time.time;
        }
    }

    // ─── Shooting ───
    private void HandleShooting()
    {
        if (projectilePrefab == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + scaledFireInterval;

        // Hard mode (wave 10+): bắn thêm spread shot 2 bên
        if (isHardMode)
            FireSpread();
        else
            FireSingle();
    }

    private void FireSingle()
    {
        Vector2 dir = GetAimedDirection();
        SpawnBullet(dir);
    }

    private void FireSpread()
    {
        // Bắn 3 đạn: giữa (aim) + 2 bên (±20 độ)
        Vector2 centerDir = GetAimedDirection();
        SpawnBullet(centerDir);
        SpawnBullet(Quaternion.Euler(0, 0,  20f) * centerDir);
        SpawnBullet(Quaternion.Euler(0, 0, -20f) * centerDir);
    }

    /// <summary>
    /// Tính hướng bắn: blend giữa thẳng xuống và hướng đến player
    /// aimStrength = 0 → Vector2.down, aimStrength = 1 → full aim
    /// </summary>
    private Vector2 GetAimedDirection()
    {
        Vector2 baseDir = Vector2.down;

        // Tìm player để aim
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null) return baseDir;

        // Thêm aim scaling theo wave (càng cao càng aim chuẩn hơn)
        float waveAimBonus = Mathf.Min((waveNumber - 1) * 0.04f, 0.4f);
        float totalAim = Mathf.Clamp01(aimStrength + waveAimBonus);

        Vector2 toPlayer = ((Vector2)playerGO.transform.position - (Vector2)transform.position).normalized;
        return Vector2.Lerp(baseDir, toPlayer, totalAim).normalized;
    }

    private void SpawnBullet(Vector2 dir)
    {
        Vector3 spawnPos = shootPoint != null
            ? shootPoint.position
            : transform.position + Vector3.down * 0.5f;

        var bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var proj   = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(dir);
            proj.SetAsEnemyProjectile();
        }

        if (shootSound != null)
            AudioSource.PlayClipAtPoint(shootSound, transform.position, 0.35f);
    }

    // ─── Death ───
    private void HandleDeath()
    {
        // FIX: Guard chống double-kill
        if (isDead) return;
        isDead = true;

        if (explosionPrefab != null)
        {
            var explo = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explo, 2f);
        }

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 0.7f);

        if (GameManager.Instance != null)
            GameManager.Instance.AddEnemyKill();

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var playerDamageable = other.GetComponent<Damageable>();
            if (playerDamageable != null)
                playerDamageable.TakeDamage(contactDamage);
        }
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDeath -= HandleDeath;
    }

    // ─── Gizmo (debug) ───
    private void OnDrawGizmosSelected()
    {
        // Hiện camera top line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-10f, cameraTopY, 0f), new Vector3(10f, cameraTopY, 0f));
    }
}
