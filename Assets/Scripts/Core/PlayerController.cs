using UnityEngine;

/// <summary>
/// Player ship - di chuyển WASD/Arrow + bắn Space/Auto
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;

    [Header("Bounds")]
    [SerializeField] private float boundaryPadding = 0.5f;

    private Damageable damageable;
    private AudioSource audioSource;
    private float nextFireTime;
    private Camera mainCam;
    private Vector2 screenBoundsMin;
    private Vector2 screenBoundsMax;

    private void Awake()
    {
        damageable = GetComponent<Damageable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        mainCam = Camera.main;
        CalculateBounds();

        if (damageable != null)
        {
            damageable.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, v, 0f).normalized * moveSpeed * Time.deltaTime;
        transform.position += move;

        // Clamp to screen bounds
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, screenBoundsMin.x, screenBoundsMax.x);
        pos.y = Mathf.Clamp(pos.y, screenBoundsMin.y, screenBoundsMax.y);
        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null || shootPoint == null) return;

        Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, 0.5f);
        }
    }

    private void HandleDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        gameObject.SetActive(false);
    }

    private void CalculateBounds()
    {
        if (mainCam == null) return;
        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        screenBoundsMin = new Vector2(bottomLeft.x + boundaryPadding, bottomLeft.y + boundaryPadding);
        screenBoundsMax = new Vector2(topRight.x - boundaryPadding, topRight.y - boundaryPadding);
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDeath -= HandleDeath;
    }
}
