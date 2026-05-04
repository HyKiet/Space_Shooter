using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player ship - di chuyển WASD/Arrow + bắn Space/J
/// Hỗ trợ 4 WeaponType: Blaster, Laser, Missile, Plasma
/// WeaponLevel 1-3 áp dụng cho tất cả loại vũ khí (số lượng đạn)
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ─── Weapon Type Enum ───
    public enum WeaponType { Blaster, Laser, Missile, Plasma }

    [Header("Movement")]
    [SerializeField] private float moveSpeed  = 8f;
    [SerializeField] private float smoothTime = 0.08f;

    [Header("Shooting — Base")]
    [SerializeField] private float fireRate   = 0.2f;
    [SerializeField] private float spreadAngle = 15f;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private Transform shootPoint;

    [Header("Weapon Prefabs")]
    [SerializeField] private GameObject blasterPrefab;
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private GameObject plasmaPrefab;

    [Header("Weapon Level")]
    [SerializeField] private int maxWeaponLevel = 3;

    [Header("Bounds")]
    [SerializeField] private float boundaryPadding = 0.5f;

    // ─── Private State ───
    private Damageable   damageable;
    private AudioSource  audioSource;
    private float        nextFireTime;
    private Camera       mainCam;
    private Vector2      screenBoundsMin;
    private Vector2      screenBoundsMax;

    private int        weaponLevel = 1;
    private WeaponType currentWeapon = WeaponType.Blaster;

    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private Vector2 moveInput;
    private bool    isShooting;

    // ─── Properties ───
    public int        WeaponLevel   => weaponLevel;
    public WeaponType CurrentWeapon => currentWeapon;

    // ─── Events (cho WeaponHUD subscribe) ───
    public event System.Action<WeaponType> OnWeaponTypeChanged;
    public event System.Action<int>        OnWeaponLevelChanged;

    // ─── Lifecycle ───
    private void Awake()
    {
        damageable  = GetComponent<Damageable>();
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
            damageable.OnDeath += HandleDeath;

        // Fire events lần đầu để HUD hiển thị đúng trạng thái ban đầu
        OnWeaponTypeChanged?.Invoke(currentWeapon);
        OnWeaponLevelChanged?.Invoke(weaponLevel);
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        ReadInput();
        HandleMovement();
        HandleShooting();
    }

    // ─── Input ───
    private void ReadInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = 0f, v = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h =  1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v =  1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v = -1f;

        moveInput  = new Vector2(h, v);
        isShooting = kb.spaceKey.isPressed || kb.jKey.isPressed;
    }

    // ─── Movement ───
    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0f).normalized * moveSpeed * Time.deltaTime;
        targetPosition += move;
        targetPosition.x = Mathf.Clamp(targetPosition.x, screenBoundsMin.x, screenBoundsMax.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, screenBoundsMin.y, screenBoundsMax.y);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // ─── Shooting ───
    private void HandleShooting()
    {
        if (isShooting && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + GetFireRate();
        }
    }

    /// <summary>Fire rate thay đổi theo loại vũ khí</summary>
    private float GetFireRate()
    {
        switch (currentWeapon)
        {
            case WeaponType.Laser:   return fireRate * 0.6f;  // Bắn nhanh hơn
            case WeaponType.Missile: return fireRate * 1.5f;  // Bắn chậm hơn
            case WeaponType.Plasma:  return fireRate * 2.0f;  // Bắn chậm nhất
            default:                 return fireRate;
        }
    }

    private void Fire()
    {
        var prefab = GetCurrentPrefab();
        if (prefab == null || shootPoint == null) return;

        switch (weaponLevel)
        {
            case 1:
                SpawnBullet(prefab, Vector2.up);
                break;
            case 2:
                SpawnBullet(prefab, Quaternion.Euler(0, 0,  spreadAngle * 0.5f) * Vector2.up);
                SpawnBullet(prefab, Quaternion.Euler(0, 0, -spreadAngle * 0.5f) * Vector2.up);
                break;
            default:
                SpawnBullet(prefab, Vector2.up);
                SpawnBullet(prefab, Quaternion.Euler(0, 0,  spreadAngle) * Vector2.up);
                SpawnBullet(prefab, Quaternion.Euler(0, 0, -spreadAngle) * Vector2.up);
                break;
        }

        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.5f);
    }

    private void SpawnBullet(GameObject prefab, Vector2 dir)
    {
        var bullet = Instantiate(prefab, shootPoint.position, Quaternion.identity);
        var proj   = bullet.GetComponent<Projectile>();
        if (proj != null)
            proj.SetDirection(dir);
    }

    private GameObject GetCurrentPrefab()
    {
        switch (currentWeapon)
        {
            case WeaponType.Laser:   return laserPrefab  != null ? laserPrefab  : blasterPrefab;
            case WeaponType.Missile: return missilePrefab != null ? missilePrefab : blasterPrefab;
            case WeaponType.Plasma:  return plasmaPrefab  != null ? plasmaPrefab  : blasterPrefab;
            default:                 return blasterPrefab;
        }
    }

    // ─── Public API ───

    /// <summary>Được gọi bởi WeaponUpgrade pickup — tăng level hiện tại</summary>
    public void UpgradeWeapon()
    {
        if (weaponLevel < maxWeaponLevel)
        {
            weaponLevel++;
            OnWeaponLevelChanged?.Invoke(weaponLevel);
            Debug.Log($"[PlayerController] Weapon level up → {weaponLevel}");
        }
    }

    /// <summary>Được gọi bởi weapon-type pickup — đổi loại vũ khí</summary>
    public void SetWeaponType(WeaponType type)
    {
        currentWeapon = type;
        weaponLevel   = 1; // Reset level về 1 khi đổi vũ khí
        OnWeaponTypeChanged?.Invoke(type);
        OnWeaponLevelChanged?.Invoke(weaponLevel);
        Debug.Log($"[PlayerController] Weapon switched → {type}");
    }

    // ─── Death ───
    private void HandleDeath()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
        gameObject.SetActive(false);
    }

    private void CalculateBounds()
    {
        if (mainCam == null) return;
        Vector3 bl = mainCam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 tr = mainCam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        screenBoundsMin = new Vector2(bl.x + boundaryPadding, bl.y + boundaryPadding);
        screenBoundsMax = new Vector2(tr.x - boundaryPadding, tr.y - boundaryPadding);
    }

    private void OnDestroy()
    {
        if (damageable != null)
            damageable.OnDeath -= HandleDeath;
    }
}
