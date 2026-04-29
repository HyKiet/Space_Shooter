using UnityEngine;

/// <summary>
/// Tự hủy GameObject sau khi animation chạy xong
/// Gắn vào explosion prefab để đảm bảo chỉ nổ 1 lần
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.5f;

    private void Start()
    {
        // Nếu có Animator, lấy thời gian animation
        Animator anim = GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            var clips = anim.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
            {
                lifetime = clips[0].length;
            }
        }

        Destroy(gameObject, lifetime);
    }
}
