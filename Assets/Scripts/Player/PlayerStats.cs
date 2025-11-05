
using UnityEngine;
using System;

/// <summary>
/// Manages the player's core stats like Health and FUN.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("FUN Meter")]
    [SerializeField] private float maxFun = 100f;
    private float currentFun;

    // Events to notify other systems (like the UI) of changes.
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnFunChanged;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentFun => currentFun;
    public float MaxFun => maxFun;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentFun = 0; // Start with no FUN
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Gain a small amount of FUN when taking damage
        AddFun(amount * 0.25f); // Example: gain 25% of damage taken as FUN
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddFun(float amount)
    {
        if (amount <= 0) return;

        currentFun = Mathf.Min(maxFun, currentFun + amount);
        OnFunChanged?.Invoke(currentFun, maxFun);
    }

    public bool TryUseFun(float amount)
    {
        if (currentFun < amount)
        {
            return false;
        }

        currentFun -= amount;
        OnFunChanged?.Invoke(currentFun, maxFun);
        return true;
    }
}
