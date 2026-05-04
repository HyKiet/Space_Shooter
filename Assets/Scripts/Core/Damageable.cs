using UnityEngine;
using System;

/// <summary>
/// Component HP chung - dùng cho Player, Enemy, Meteor
/// </summary>
public class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => currentHealth <= 0f;

    // Shield / Invincibility
    private bool isInvincible = false;
    public bool IsInvincible => isInvincible;
    public void SetInvincible(bool value) => isInvincible = value;

    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDeath;
    public event Action OnDamagePrevented; // Fire khi shield block damage

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        // Shield block
        if (isInvincible)
        {
            OnDamagePrevented?.Invoke();
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Camera shake khi bị damage
        if (CameraShake.Instance != null && gameObject.CompareTag("Player"))
        {
            if (currentHealth <= 0f)
                CameraShake.Instance.ShakeHeavy();
            else
                CameraShake.Instance.ShakeMedium();
        }

        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = value;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
