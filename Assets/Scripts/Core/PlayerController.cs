using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player ship - di chuyển WASD/Arrow + bắn Space/J
/// Sử dụng New Input System
/// Hỗ trợ weapon upgrade level 1-3
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.08f; // Độ trễ di chuyển (smooth damp)

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private AudioClip shootSound;

    [Header("Weapon Upgrade")]
    [SerializeField] private int maxWeaponLevel = 3;
    [SerializeField] private float spreadAngle = 15f;

    [Header("Bounds")]
    [SerializeField] private float boundaryPadding = 0.5f;

    private Damageable damageable;
    private AudioSource audioSource;
    private float nextFireTime;
    private Camera mainCam;
    private Vector2 screenBoundsMin;
    private Vector2 screenBoundsMax;

    // Weapon level: 1 = single, 2 = double, 3 = triple fan
    private int weaponLevel = 1;

    // Smooth movement
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    // New Input System
    private Vector2 moveInput;
    private bool isShooting;

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
        targetPosition = transform.position;

        if (damageable != null)
        {
            damageable.OnDeath += HandleDeath;
        }
    }

    private void Update()
    {
        // Allow input if GameManager not initialized yet OR state is Playing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        ReadInput();
        HandleMovement();
        HandleShooting();
    }

    private void ReadInput()
    {
        // New Input System - đọc keyboard trực tiếp
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = 0f;
        float v = 0f;

        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h = 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v = 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v = -1f;

        moveInput = new Vector2(h, v);
        isShooting = kb.spaceKey.isPressed || kb.jKey.isPressed;
    }

    private void HandleMovement()
    {
        // Tính vị trí mục tiêu
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0f).normalized * moveSpeed * Time.deltaTime;
        targetPosition += move;

        // Clamp target to screen bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, screenBoundsMin.x, screenBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, screenBoundsMin.y, screenBoundsMax.y);

        // Smooth damp - tạo độ trễ mượt mà
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void HandleShooting()
    {
        if (isShooting && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        if (projectilePrefab == null || shootPoint == null)
            return;

        switch (weaponLevel)
        {
            case 1:
                // Single shot
                SpawnBullet(Vector2.up);
                break;
            case 2:
                // Double shot - slight spread
                SpawnBullet(Quaternion.Euler(0, 0, spreadAngle * 0.5f) * Vector2.up);
                SpawnBullet(Quaternion.Euler(0, 0, -spreadAngle * 0.5f) * Vector2.up);
                break;
            default:
                // Triple fan
                SpawnBullet(Vector2.up);
                SpawnBullet(Quaternion.Euler(0, 0, spreadAngle) * Vector2.up);
                SpawnBullet(Quaternion.Euler(0, 0, -spreadAngle) * Vector2.up);
                break;
        }

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, 0.5f);
        }
    }

    private void SpawnBullet(Vector2 direction)
    {
        var bullet = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
        var proj = bullet.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.SetDirection(direction);
        }
    }

    /// <summary>
    /// Gọi bởi ItemPickup khi player nhặt weapon upgrade
    /// </summary>
    public void UpgradeWeapon()
    {
        if (weaponLevel < maxWeaponLevel)
        {
            weaponLevel++;
            Debug.Log($"[PlayerController] Weapon upgraded to level {weaponLevel}");
        }
    }

    public int WeaponLevel => weaponLevel;

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
