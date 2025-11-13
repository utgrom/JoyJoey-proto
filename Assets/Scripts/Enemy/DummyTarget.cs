using UnityEngine;

public class DummyTarget : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    private float health;

    private void Awake() { health = maxHealth; }

    public void TakeDamage(float amount, GameObject source)
    {
        health -= amount;
        Debug.Log($"Dummy recibió {amount}. Vida: {health}");
        if (health <= 0) Debug.Log("Dummy derrotado");
    }
}
