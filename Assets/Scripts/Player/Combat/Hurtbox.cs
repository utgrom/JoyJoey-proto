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

    public event Action<HitContext> OnHitApplied;

    private float currentArmor;
    private float timeSinceLastHit;
    private bool isGrounded;
    private PlayerMovement playerMovement;

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
}
