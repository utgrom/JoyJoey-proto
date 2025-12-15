using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour, IDamageable
{
    [Header("Refs")]
    [SerializeField] private Health health;
    [SerializeField] private Rigidbody2D rb;

    [Header("Weights")]
    [SerializeField] private float weightHorizontal = 1f;
    [SerializeField] private float weightVertical = 1f;

    [Header("Armor (enemies)")]
    [SerializeField] private bool useArmor = false;
    [SerializeField] private bool armorInfinite = false;
    [SerializeField] private float armorMax = 20f;
    [SerializeField] private float armorRegenDelay = 1.5f;
    [SerializeField] private float armorRegenPerSecond = 5f;

    [Header("Hitstun Gravity")]
    [SerializeField] private bool reduceGravityInHitstun = true;
    [SerializeField] [Range(0.05f, 1f)] private float hitstunGravityFactor = 0.33f;

    public event Action<HitContext> OnHitApplied;

    private float currentArmor;
    private float timeSinceLastHit;
    private bool isGrounded;
    private PlayerMovement playerMovement;
    private float hitstunTimer;
    private float originalGravityScale = -1f;
    private bool gravityReduced;

    private void Awake()
    {
        if (!health) health = GetComponent<Health>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        currentArmor = armorMax;
    }

    private void Update()
    {
        if (playerMovement != null)
        {
            isGrounded = playerMovement.IsGrounded;
        }

        timeSinceLastHit += Time.deltaTime;

        if (useArmor && !armorInfinite && isGrounded && timeSinceLastHit >= armorRegenDelay)
        {
            currentArmor = Mathf.Min(armorMax, currentArmor + armorRegenPerSecond * Time.deltaTime);
        }

        if (hitstunTimer > 0f)
        {
            hitstunTimer -= Time.deltaTime;
            if (hitstunTimer <= 0f)
            {
                RestoreGravity();
            }
        }
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    public void ApplyHit(HitContext context)
    {
        if (health != null && health.IsDead) return;
        if (health != null && context.payload.ignoreIFrames == false)
        {
            // Health handles its own i-frames; calling TakeDamage will early out if active.
        }

        bool armorBlocked = false;

        if (useArmor && !armorInfinite)
        {
            float breakAmount = context.payload.damage + context.payload.armorBreak;
            if (currentArmor > 0f)
            {
                currentArmor -= breakAmount;
                armorBlocked = currentArmor > 0f;
            }
        }

        timeSinceLastHit = 0f;

        bool tookDamage = health != null ? health.TakeDamage(context.payload.damage) : true;

        if (!armorBlocked && tookDamage)
        {
            ApplyKnockback(context);
            HandleHitstunGravity(context.payload);
            OnHitApplied?.Invoke(context);
        }
    }

    private void ApplyKnockback(HitContext context)
    {
        if (rb == null) return;

        Vector2 kb = KnockbackSolver.Solve(context, weightHorizontal, weightVertical);

        if (context.payload.resetVerticalVelocity)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        if (context.payload.inheritSourceVerticalVelocity)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, context.sourceVelocity.y);
        }

        rb.linearVelocity = new Vector2(kb.x, kb.y);
    }

    private void HandleHitstunGravity(HitPayload payload)
    {
        if (!reduceGravityInHitstun || rb == null) return;
        if (payload.hitstunSeconds <= 0f) return;
        // Solo reducir gravedad en golpes que resetean vel. vertical
        if (!payload.resetVerticalVelocity) return;

        hitstunTimer = Mathf.Max(hitstunTimer, payload.hitstunSeconds);

        if (!gravityReduced)
        {
            originalGravityScale = rb.gravityScale;
            rb.gravityScale = originalGravityScale * hitstunGravityFactor;
            gravityReduced = true;
        }
    }

    private void RestoreGravity()
    {
        if (!gravityReduced || rb == null) return;
        if (originalGravityScale > 0f)
        {
            rb.gravityScale = originalGravityScale;
        }
        gravityReduced = false;
        hitstunTimer = 0f;
    }
}
