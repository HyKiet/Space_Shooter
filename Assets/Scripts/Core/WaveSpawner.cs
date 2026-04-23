using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Spawner - spawn waves enemy + meteor tăng dần theo wave
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int baseEnemyCount = 3;
    [SerializeField] private int enemiesPerWaveIncrease = 1;
    [SerializeField] private float enemySpawnDelay = 0.5f;

    [Header("Meteor Spawning")]
    [SerializeField] private GameObject[] meteorPrefabs;
    [SerializeField] private int baseMeteorCount = 1;
    [SerializeField] private float meteorSpawnInterval = 3f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float spawnXMin = -7f;
    [SerializeField] private float spawnXMax = 7f;

    [Header("Wave Timing")]
    [SerializeField] private float waveCooldown = 3f;
    [SerializeField] private float initialDelay = 2f;

    private int currentWave = 0;
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

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

            int enemyCount = baseEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;

            yield return StartCoroutine(SpawnWave(enemyCount));

            // Wait until all enemies are destroyed
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

    private IEnumerator SpawnWave(int count)
    {
        isSpawning = true;

        for (int i = 0; i < count; i++)
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                yield break;

            float x = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPos = new Vector3(x, spawnY, 0f);

            if (enemyPrefab != null)
            {
                var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                activeEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(enemySpawnDelay);
        }

        isSpawning = false;
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

            if (meteorPrefabs != null && meteorPrefabs.Length > 0)
            {
                float x = Random.Range(spawnXMin, spawnXMax);
                Vector3 spawnPos = new Vector3(x, spawnY + 1f, 0f);

                var meteorPrefab = meteorPrefabs[Random.Range(0, meteorPrefabs.Length)];
                if (meteorPrefab != null)
                {
                    Instantiate(meteorPrefab, spawnPos, Quaternion.identity);
                }
            }

            // Decrease interval slightly as waves increase
            float interval = Mathf.Max(1f, meteorSpawnInterval - (currentWave * 0.2f));
            yield return new WaitForSeconds(interval);
        }
    }
}
