using UnityEngine;

public interface IDamageable { void TakeDamage(float amount, GameObject source); }


[RequireComponent(typeof(Collider2D))]
public class Hitbox2D : MonoBehaviour
{
    public float damage = 10f;
    public GameObject owner;
    public LayerMask targetLayers;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        var d = other.GetComponent<IDamageable>();
        if (d != null) d.TakeDamage(damage, owner);
    }
}
