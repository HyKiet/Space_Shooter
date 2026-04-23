using UnityEngine;

/// <summary>
/// Tự huỷ sau X giây - dùng cho explosion effects
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
