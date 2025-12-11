using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileMover : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeSeconds = 3f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private HitPayload payload;

    private Vector2 direction = Vector2.right;
    private GameObject owner;
    private float timer;

    public void Init(GameObject ownerObj, Vector2 facingDir)
    {
        owner = ownerObj;
        direction = facingDir.sqrMagnitude > 0.01f ? facingDir.normalized : Vector2.right;
        timer = lifeSeconds;
    }

    private void Awake()
    {
        timer = lifeSeconds;
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var dmg = other.GetComponent<IDamageable>();
        if (dmg == null) return;

        Vector2 srcPos = owner != null ? (Vector2)owner.transform.position : (Vector2)transform.position;
        Vector2 srcVel = Vector2.zero;
        var rb = owner != null ? owner.GetComponent<Rigidbody2D>() : null;
        if (rb != null) srcVel = rb.linearVelocity;

        var ctx = new HitContext
        {
            payload = payload,
            source = owner != null ? owner : gameObject,
            sourcePosition = srcPos,
            sourceVelocity = srcVel,
            facing = direction
        };

        dmg.ApplyHit(ctx);
        Destroy(gameObject);
    }
}
