using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HitboxInstance : MonoBehaviour
{
    private HitboxConfig config;
    private GameObject owner;
    private Vector2 facing = Vector2.right;
    private float lifeTimer;
    private Collider2D col;

    public void Init(HitboxConfig cfg, GameObject ownerObj, Vector2 facingDir)
    {
        config = cfg;
        owner = ownerObj;
        facing = facingDir.sqrMagnitude > 0.01f ? facingDir.normalized : Vector2.right;
        lifeTimer = cfg.lifeSeconds;
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (config.radius > 0f && col is CircleCollider2D circle)
        {
            circle.radius = config.radius;
        }

        ApplyOffset();
    }

    private void Update()
    {
        if (config == null) return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyOffset()
    {
        if (config == null) return;
        Vector2 offset = config.localOffset;
        if (config.flipWithFacing && facing.x < 0f)
        {
            offset.x *= -1f;
        }

        if (config.attachToOwner && owner != null)
        {
            transform.SetParent(owner.transform, worldPositionStays: false);
            transform.localPosition = offset;
        }
        else
        {
            transform.position = (Vector2)transform.position + offset;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (config == null) return;
        if ((config.targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var dmg = other.GetComponent<IDamageable>();
        if (dmg == null) return;

        Vector2 srcPos = owner != null ? (Vector2)owner.transform.position : (Vector2)transform.position;
        Vector2 srcVel = Vector2.zero;
        var rb = owner != null ? owner.GetComponent<Rigidbody2D>() : null;
        if (rb != null) srcVel = rb.linearVelocity;

        var ctx = new HitContext
        {
            payload = config.payload,
            source = owner != null ? owner : gameObject,
            sourcePosition = srcPos,
            sourceVelocity = srcVel,
            facing = facing
        };

        dmg.ApplyHit(ctx);
    }
}
