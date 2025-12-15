using UnityEngine;

/// <summary>
/// Bridges gameplay state to Animator parameters. All parameter names are optional; leave empty to skip.
/// </summary>
public class PlayerAnimationBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCombatController combat;
    [SerializeField] private Health health;
    [SerializeField] private Hurtbox hurtbox;

    [Header("Animator Parameters")]
    [SerializeField] private string runBool = "Run";
    [SerializeField] private string wallSlideBool = "WallSlide";
    [SerializeField] private string hurtTrigger = "Hurt";
    [SerializeField] private string dieTrigger = "Die";

    [Header("Tuning")]
    [SerializeField] private float runSpeedThreshold = 0.1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!combat) combat = GetComponent<PlayerCombatController>();
        if (!health) health = GetComponent<Health>();
        if (!hurtbox) hurtbox = GetComponent<Hurtbox>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDeath.AddListener(OnDeath);
        if (hurtbox != null) hurtbox.OnHitApplied += OnHurt;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDeath.RemoveListener(OnDeath);
        if (hurtbox != null) hurtbox.OnHitApplied -= OnHurt;
    }

    private void Update()
    {
        if (animator == null || movement == null) return;

        if (!string.IsNullOrEmpty(runBool) && rb != null)
        {
            bool isRunning = movement.IsGrounded && Mathf.Abs(rb.linearVelocity.x) > runSpeedThreshold;
            animator.SetBool(runBool, isRunning);
        }

        if (!string.IsNullOrEmpty(wallSlideBool))
        {
            animator.SetBool(wallSlideBool, movement.IsWallSliding);
        }
    }

    private void OnHurt(HitContext ctx)
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(hurtTrigger))
        {
            animator.SetTrigger(hurtTrigger);
        }
    }

    private void OnDeath()
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(dieTrigger))
        {
            animator.SetTrigger(dieTrigger);
        }
    }
}
