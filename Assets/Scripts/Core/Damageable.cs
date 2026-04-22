using UnityEngine;
using System;

public class Damageable : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private bool destroyOnDeath = true;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public event Action<Damageable> Died;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        if (amount <= 0f || isDead)
        {
            return;
        }

        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            isDead = true;
            Died?.Invoke(this);
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
