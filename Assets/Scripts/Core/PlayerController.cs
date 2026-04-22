using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SpaceGameInput gameInput;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float shootCooldown = 0.15f;
    [SerializeField] private bool clampToCamera = true;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private float projectileDamage = 1f;

    private Camera mainCam;
    private float lastShootTime;

    private void Awake()
    {
        mainCam = Camera.main;

        if (gameInput == null)
        {
            gameInput = FindFirstObjectByType<SpaceGameInput>();
        }
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.ShootPressed += HandleShoot;
        }
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.ShootPressed -= HandleShoot;
        }
    }

    private void Update()
    {
        Vector2 input = gameInput != null ? gameInput.Move : Vector2.zero;
        Vector3 delta = new Vector3(input.x, input.y, 0f) * (moveSpeed * Time.deltaTime);
        transform.position += delta;

        if (clampToCamera)
        {
            ClampInsideCamera();
        }
    }

    private void ClampInsideCamera()
    {
        if (mainCam == null)
        {
            return;
        }

        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);
        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);
        transform.position = mainCam.ViewportToWorldPoint(viewPos);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    private void HandleShoot()
    {
        if (Time.time - lastShootTime < shootCooldown)
        {
            return;
        }

        lastShootTime = Time.time;

        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerController: Missing projectilePrefab.", this);
            return;
        }

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position + Vector3.up * 0.9f;
        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Vector2 shootDirection = Vector2.up;
        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(shootDirection, projectileSpeed, projectileLifetime, projectileDamage, gameObject);
        }
    }
}
