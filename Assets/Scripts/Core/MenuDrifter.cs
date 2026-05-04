using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component gắn vào từng vật thể trôi nổi để xử lý di chuyển và xoay
/// </summary>
public class MenuDrifter : MonoBehaviour
{
    private Vector3 velocity;
    private float rotationSpeed;
    private Camera mainCam;

    public void Initialize(Vector3 vel, float rotSpeed)
    {
        velocity = vel;
        rotationSpeed = rotSpeed;
        mainCam = Camera.main;
    }

    private void Update()
    {
        // Di chuyển
        transform.position += velocity * Time.deltaTime;
        
        // Xoay
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Hủy nếu bay ra khỏi màn hình
        if (mainCam != null)
        {
            float height = mainCam.orthographicSize;
            float width = height * mainCam.aspect;
            float pad = 12f; // Tăng padding cực lớn cho các thiên thạch khổng lồ

            if (transform.position.x > width + pad || transform.position.x < -width - pad ||
                transform.position.y > height + pad || transform.position.y < -height - pad)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            mainCam = Camera.main;
        }
    }
}

/// <summary>
/// Quản lý việc sinh ra các vật thể trôi nổi trong Menu
/// </summary>
public class MenuDrifterSpawner : MonoBehaviour
{
    [Header("Prefabs to Spawn")]
    [Tooltip("Kéo thả prefab thiên thạch hoặc tàu địch vào đây")]
    public GameObject[] prefabsToSpawn;

    [Header("Settings")]
    public float spawnInterval = 1.2f;
    public float minSpeed = 1f;
    public float maxSpeed = 3f;
    public float minRotation = -50f;
    public float maxRotation = 50f;

    [Header("Scale Settings")]
    public float minScale = 1.5f;
    public float maxScale = 3.5f;

    private List<GameObject> activeDrifters = new List<GameObject>();
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnEnable()
    {
        StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Sinh ra nếu ở Menu hoặc nếu không có GameManager (đang test scene)
            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameManager.GameState.Menu)
            {
                SpawnDrifter();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnDrifter()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0) return;

        GameObject prefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
        if (prefab == null) return;

        float height = mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        float spawnX = 0f;
        float spawnY = 0f;
        Vector3 velocity = Vector3.zero;

        int side = Random.Range(0, 4);
        float pad = 10f; // Xuất phát từ xa hơn để không bị giật hình khi xuất hiện (vì object rất to)

        // Xác định vị trí xuất phát từ rìa màn hình
        switch (side)
        {
            case 0: // Trên
                spawnX = Random.Range(-width, width);
                spawnY = height + pad;
                velocity = new Vector3(Random.Range(-1f, 1f), -Random.Range(0.5f, 1f), 0).normalized;
                break;
            case 1: // Phải
                spawnX = width + pad;
                spawnY = Random.Range(-height, height);
                velocity = new Vector3(-Random.Range(0.5f, 1f), Random.Range(-1f, 1f), 0).normalized;
                break;
            case 2: // Dưới
                spawnX = Random.Range(-width, width);
                spawnY = -height - pad;
                velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), 0).normalized;
                break;
            case 3: // Trái
                spawnX = -width - pad;
                spawnY = Random.Range(-height, height);
                velocity = new Vector3(Random.Range(0.5f, 1f), Random.Range(-1f, 1f), 0).normalized;
                break;
        }

        // Đẩy ra phía sau (z = 10)
        Vector3 startPos = new Vector3(spawnX, spawnY, 10f);
        GameObject drifterObj = Instantiate(prefab, startPos, Quaternion.identity);
        activeDrifters.Add(drifterObj);

        // Vô hiệu hóa tất cả Collider và các Script game logic để chúng chỉ là vật thể hình ảnh
        Collider2D[] colliders = drifterObj.GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders) col.enabled = false;

        MonoBehaviour[] scripts = drifterObj.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script is MenuDrifter) continue;
            script.enabled = false;
        }

        Rigidbody2D rb = drifterObj.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Thêm script điều khiển chuyển động
        MenuDrifter drifter = drifterObj.AddComponent<MenuDrifter>();
        float speed = Random.Range(minSpeed, maxSpeed);
        float rotSpeed = Random.Range(minRotation, maxRotation);
        drifter.Initialize(velocity * speed, rotSpeed);

        // Ngẫu nhiên kích thước (Làm cho thiên thạch bự giống ảnh)
        float scale = Random.Range(minScale, maxScale);
        drifterObj.transform.localScale = Vector3.one * scale;

        // Đảm bảo hiển thị trên nền nhưng dưới UI
        SpriteRenderer[] srs = drifterObj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            sr.sortingOrder = 5; // Cao hơn các ngôi sao (-10)
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        // Hủy tất cả vật thể trôi nổi khi vào chơi game
        if (state != GameManager.GameState.Menu)
        {
            foreach (var drifter in activeDrifters)
            {
                if (drifter != null) Destroy(drifter);
            }
            activeDrifters.Clear();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
