using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawner - spawn waves enemy + meteor tăng dần theo wave
/// Sau Wave 5: Spawn Boss C7 với warning screen
/// Sau boss chết: tiếp tục wave 6+
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    // ─── Enemy Spawn Entry ─────────────────────────────────
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public GameObject prefab;
        [Tooltip("Xuất hiện từ wave nào trở đi")]
        public int minWave = 1;
        [Tooltip("Tỷ lệ spawn so với các loại khác (cao = xuất hiện nhiều hơn)")]
        public float weight = 1f;
    }

    [Header("Enemy Spawning")]
    [Tooltip("Danh sách enemy types — mỗi entry có minWave và weight riêng")]
    [SerializeField] private EnemySpawnEntry[] enemyTypes;
    [SerializeField] private int   baseEnemyCount        = 3;
    [SerializeField] private int   enemiesPerWaveIncrease = 1;
    [SerializeField] private float enemySpawnDelay       = 0.5f;

    [Header("Meteor Spawning")]
    [SerializeField] private GameObject[] meteorPrefabs;
    [SerializeField] private float meteorSpawnInterval = 3f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float spawnXMin = -7f;
    [SerializeField] private float spawnXMax = 7f;

    [Header("Wave Timing")]
    [SerializeField] private float waveCooldown = 3f;
    [SerializeField] private float initialDelay = 2f;

    [Header("Boss C7")]
    [SerializeField] private GameObject bossC7Prefab;
    [SerializeField] private int bossWaveInterval = 5;      // Boss xuất hiện mỗi N wave
    [SerializeField] private BossWarningUI bossWarningUI;   // Kéo vào Inspector
    [SerializeField] private BossHealthBarUI bossHealthBarUI; // Kéo vào Inspector
    [SerializeField] private string bossDisplayName = "C7 — Ưng Thép";

    private int currentWave = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isBossAlive = false;

    private void Start()
    {
        StartCoroutine(WaveLoop());
        StartCoroutine(MeteorSpawnLoop());
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                yield return null;
                continue;
            }

            currentWave++;
            if (GameManager.Instance != null)
                GameManager.Instance.SetWave(currentWave);

            // ===== Boss Wave Check =====
            if (bossC7Prefab != null && currentWave % bossWaveInterval == 0)
            {
                yield return StartCoroutine(BossWaveRoutine());
                yield return new WaitForSeconds(waveCooldown);
                continue;
            }

            // ===== Normal Wave =====
            int enemyCount = baseEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;
            yield return StartCoroutine(SpawnWave(enemyCount));

            // Chờ hết enemies
            yield return new WaitUntil(() =>
            {
                activeEnemies.RemoveAll(e => e == null);
                return activeEnemies.Count == 0 ||
                       (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing);
            });

            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                yield break;

            yield return new WaitForSeconds(waveCooldown);
        }
    }

    // ───────── Boss Wave Routine ─────────
    private IEnumerator BossWaveRoutine()
    {
        // Xóa hết enemies đang còn
        CleanupAllEnemies();

        // 1. Hiện WARNING screen
        if (bossWarningUI != null)
        {
            bool warningDone = false;
            bossWarningUI.ShowWarning(bossDisplayName, () => warningDone = true);
            yield return new WaitUntil(() => warningDone);
        }
        else
        {
            // Fallback: chờ 2s
            yield return new WaitForSeconds(2f);
        }

        // 2. Spawn Boss
        if (bossC7Prefab == null) yield break;

        var bossGO = Instantiate(bossC7Prefab, new Vector3(0f, 9f, 0f), Quaternion.identity);
        var boss = bossGO.GetComponent<BossController>();

        if (boss == null) yield break;

        // Thêm collider với bullet để damage
        AddBulletDamageHandler(bossGO, boss);

        isBossAlive = true;
        GameManager.Instance?.NotifyBossSpawned();

        // 3. Báo cho HP Bar track boss
        if (bossHealthBarUI != null)
            bossHealthBarUI.TrackBoss(boss, bossDisplayName);

        // 4. Subscribe sự kiện boss chết
        boss.OnDeath += () => isBossAlive = false;

        // 5. Chờ boss chết
        yield return new WaitUntil(() =>
        {
            return !isBossAlive ||
                   (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing);
        });

        yield return new WaitForSeconds(1.5f); // Pause nhỏ sau boss chết
    }

    /// <summary>
    /// Thêm Damageable + BossDamageProxy để đạn player damage boss qua Projectile system
    /// </summary>
    private void AddBulletDamageHandler(GameObject bossGO, BossController boss)
    {
        // Đảm bảo có Damageable để Projectile.OnTriggerEnter2D có thể gọi TakeDamage
        var damageable = bossGO.GetComponent<Damageable>();
        if (damageable == null)
            damageable = bossGO.AddComponent<Damageable>();

        // SetMaxHealth để Damageable không die riêng - proxy sẽ forward sang BossController
        damageable.SetMaxHealth(99999f);

        // BossDamageProxy intercepts damage → forward sang BossController
        var proxy = bossGO.AddComponent<BossDamageProxy>();
        proxy.Init(boss, damageable);
    }

    private void CleanupAllEnemies()
    {
        foreach (var e in activeEnemies)
            if (e != null) Destroy(e);
        activeEnemies.Clear();

        // Xóa tất cả enemy còn lại trong scene
        var allEnemies = GameObject.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var e in allEnemies)
            Destroy(e.gameObject);
    }

    private IEnumerator SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                yield break;

            float x = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPos = new Vector3(x, spawnY, 0f);

            // Chọn enemy type dựa theo wave và weight
            var selectedPrefab = PickEnemyPrefab(currentWave);
            if (selectedPrefab != null)
            {
                // Dùng prefab.transform.rotation để giữ flipY/rotation đã set trong prefab
                var enemy = Instantiate(selectedPrefab, spawnPos, selectedPrefab.transform.rotation);
                activeEnemies.Add(enemy);

                // AI v2: Truyền wave number để enemy tự scale difficulty
                var ec = enemy.GetComponent<EnemyController>();
                if (ec != null) ec.Init(currentWave);
            }

            yield return new WaitForSeconds(enemySpawnDelay);
        }
    }

    /// <summary>
    /// Chọn ngẫu nhiên enemy prefab phù hợp với wave hiện tại (weighted random)
    /// </summary>
    private GameObject PickEnemyPrefab(int wave)
    {
        if (enemyTypes == null || enemyTypes.Length == 0) return null;

        // Lọc các entry đủ điều kiện (minWave ≤ wave hiện tại)
        float totalWeight = 0f;
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            var e = enemyTypes[i];
            if (e.prefab != null && wave >= e.minWave)
                totalWeight += e.weight;
        }

        if (totalWeight <= 0f) return enemyTypes[0].prefab; // fallback

        // Weighted random pick
        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < enemyTypes.Length; i++)
        {
            var e = enemyTypes[i];
            if (e.prefab == null || wave < e.minWave) continue;
            cumulative += e.weight;
            if (roll <= cumulative)
                return e.prefab;
        }

        return enemyTypes[0].prefab; // fallback
    }

    private IEnumerator MeteorSpawnLoop()
    {
        yield return new WaitForSeconds(initialDelay + 2f);

        while (true)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                yield return null;
                continue;
            }

            // Không spawn meteor trong boss wave
            if (isBossAlive)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (meteorPrefabs != null && meteorPrefabs.Length > 0)
            {
                float x = Random.Range(spawnXMin, spawnXMax);
                Vector3 spawnPos = new Vector3(x, spawnY + 1f, 0f);

                var meteorPrefab = meteorPrefabs[Random.Range(0, meteorPrefabs.Length)];
                if (meteorPrefab != null)
                    Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
            }

            float interval = Mathf.Max(1f, meteorSpawnInterval - (currentWave * 0.2f));
            yield return new WaitForSeconds(interval);
        }
    }
}
