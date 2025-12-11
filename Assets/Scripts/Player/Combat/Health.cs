using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float iFramesDuration = 0.3f;

    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent OnDeath;

    private float currentHealth;
    private float iFrameTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (iFrameTimer > 0f)
        {
            iFrameTimer -= Time.deltaTime;
        }
    }

    public bool TakeDamage(float amount)
    {
        if (amount <= 0f) return false;
        if (IsDead) return false;
        if (iFrameTimer > 0f) return false;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        iFrameTimer = iFramesDuration;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
        return true;
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        if (IsDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
