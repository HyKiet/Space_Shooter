using UnityEngine;

public class SpaceSceneSetup : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Color cameraBackground = Color.black;
    [SerializeField] private float orthographicSize = 5f;

    [Header("Gameplay Anchors")]
    [SerializeField] private float playerSpawnY = -3.5f;
    [SerializeField] private float enemySpawnY = 5.5f;

    private void Awake()
    {
        SetupMainCamera();
        EnsureAnchor("PlayerSpawn", new Vector3(0f, playerSpawnY, 0f));
        EnsureAnchor("EnemySpawnLeft", new Vector3(-4f, enemySpawnY, 0f));
        EnsureAnchor("EnemySpawnCenter", new Vector3(0f, enemySpawnY, 0f));
        EnsureAnchor("EnemySpawnRight", new Vector3(4f, enemySpawnY, 0f));
        EnsureAnchor("MeteorSpawn", new Vector3(0f, enemySpawnY + 1f, 0f));
    }

    private void SetupMainCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("SpaceSceneSetup: Main Camera was not found.");
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = cameraBackground;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.transform.rotation = Quaternion.identity;
    }

    private void EnsureAnchor(string anchorName, Vector3 localPosition)
    {
        Transform anchor = transform.Find(anchorName);
        if (anchor == null)
        {
            var go = new GameObject(anchorName);
            anchor = go.transform;
            anchor.SetParent(transform);
        }

        anchor.localPosition = localPosition;
        anchor.localRotation = Quaternion.identity;
        anchor.localScale = Vector3.one;
    }
}
