using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Wave Tuning")]
    [SerializeField] private int startEnemyCount = 4;
    [SerializeField] private int enemyCountIncreasePerWave = 2;
    [SerializeField] private float startEnemySpeed = 2.2f;
    [SerializeField] private float speedIncreasePerWave = 0.35f;
    [SerializeField] private float spawnInterval = 0.6f;
    [SerializeField] private float betweenWavesDelay = 1.5f;

    private int currentWave = 0;
    private int aliveEnemies = 0;
    private bool isWaveSpawning = false;

    private void Start()
    {
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            AutoFindSpawnPoints();
        }

        if (scoreManager != null)
        {
            scoreManager.ResetRun();
        }

        StartCoroutine(WaveLoop());
    }

    public void NotifyEnemyRemoved(bool killedByPlayer, int scoreValue)
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);

        if (killedByPlayer && scoreManager != null)
        {
            scoreManager.RegisterEnemyKill(scoreValue);
        }
    }

    private IEnumerator WaveLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => !isWaveSpawning && aliveEnemies == 0);
            yield return new WaitForSeconds(betweenWavesDelay);
            yield return SpawnWave();
        }
    }

    private IEnumerator SpawnWave()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveManager: Missing enemy prefab or spawn points.", this);
            yield break;
        }

        isWaveSpawning = true;
        currentWave++;

        if (scoreManager != null)
        {
            scoreManager.SetWave(currentWave);
        }

        int enemyCount = startEnemyCount + (currentWave - 1) * enemyCountIncreasePerWave;
        float enemySpeed = startEnemySpeed + (currentWave - 1) * speedIncreasePerWave;

        for (int i = 0; i < enemyCount; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawn.position, Quaternion.identity);
            EnemyController controller = enemy.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.Initialize(this, enemySpeed);
            }

            aliveEnemies++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isWaveSpawning = false;
    }

    private void AutoFindSpawnPoints()
    {
        var left = GameObject.Find("SceneRoot/SpawnAnchors/EnemySpawnLeft")?.transform;
        var center = GameObject.Find("SceneRoot/SpawnAnchors/EnemySpawnCenter")?.transform;
        var right = GameObject.Find("SceneRoot/SpawnAnchors/EnemySpawnRight")?.transform;

        if (left != null && center != null && right != null)
        {
            spawnPoints = new[] { left, center, right };
        }
    }
}
