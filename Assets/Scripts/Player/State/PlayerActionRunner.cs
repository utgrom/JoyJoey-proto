using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerActionRunner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private HitboxSpawner hitboxSpawner;
    [SerializeField] private Animator animator;

    public bool IsPlaying => currentVariant != null;
    public bool InRecoveryPhase => IsPlaying && elapsed > (currentVariant.startup + currentVariant.active);
    public bool InActivePhase => IsPlaying && elapsed >= currentVariant.startup && elapsed < (currentVariant.startup + currentVariant.active);
    public bool BeforeRecoveryPhase => IsPlaying && elapsed < (currentVariant.startup + currentVariant.active);
    public ActionVariant CurrentVariant => currentVariant;

    private Rigidbody2D rb;
    private ActionVariant currentVariant;
    private float elapsed;
    private float activeEnd;
    private float totalEnd;
    private bool gravitySuspended;
    private float savedGravityScale;
    private Vector2 facing = Vector2.right;
    private HashSet<float> hitboxFiredTimes = new();
    private HashSet<float> motionFiredTimes = new();
    private bool cancelWindowOpen;
    private string[] currentCancelTags;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!hitboxSpawner) hitboxSpawner = GetComponent<HitboxSpawner>();
    }

    private void Update()
    {
        if (!IsPlaying) return;

        elapsed += Time.deltaTime;

        HandleHitboxes();
        HandleMotion();
        HandleCancelWindow();

        if (elapsed >= activeEnd && gravitySuspended)
        {
            ResumeGravity();
        }

        if (elapsed >= totalEnd)
        {
            EndAction();
        }
    }

    public bool TryStart(ActionVariant variant, Vector2 facingDir)
    {
        if (variant == null) return false;
        if (!CanEnter(variant)) return false;

        currentVariant = variant;
        elapsed = 0f;
        activeEnd = variant.startup + variant.active;
        totalEnd = variant.startup + variant.active + variant.recovery;
        hitboxFiredTimes.Clear();
        motionFiredTimes.Clear();
        cancelWindowOpen = false;
        facing = facingDir.sqrMagnitude > 0.01f ? facingDir.normalized : Vector2.right;
        currentCancelTags = variant.cancelTagsGranted ?? System.Array.Empty<string>();

        if (variant.zeroVelocityOnStart && rb != null)
        {
            float vx = variant.preserveHorizontalMomentum ? rb.linearVelocity.x : 0f;
            rb.linearVelocity = new Vector2(vx, 0f);
        }

        if (variant.gravityMode != GravityMode.Normal)
        {
            SuspendGravity(variant.gravityMode == GravityMode.SuspendSetYZero);
        }

        if (animator != null && !string.IsNullOrEmpty(variant.animatorTrigger))
        {
            animator.SetTrigger(variant.animatorTrigger);
        }

        return true;
    }

    public void ForceCancel()
    {
        EndAction();
    }

    public bool CanEnter(ActionVariant nextVariant)
    {
        if (!IsPlaying) return true;

        if (currentVariant == null) return true;

        bool inRecovery = InRecoveryPhase;
        bool hasCancelTag = HasAnyCancelTag(currentVariant.cancelTagsGranted, nextVariant.cancelTagsRequiredToEnter);
        bool inWindow = cancelWindowOpen || (inRecovery && currentVariant.canCancelDuringRecovery);

        return inWindow && hasCancelTag;
    }

    private bool HasAnyCancelTag(string[] granted, string[] required)
    {
        if (required == null || required.Length == 0) return true;
        if (granted == null) return false;

        foreach (var req in required)
        {
            foreach (var g in granted)
            {
                if (!string.IsNullOrEmpty(g) && g == req) return true;
            }
        }
        return false;
    }

    private void HandleHitboxes()
    {
        if (currentVariant.hitboxEvents == null || hitboxSpawner == null) return;

        foreach (var evt in currentVariant.hitboxEvents)
        {
            if (elapsed >= evt.time && !hitboxFiredTimes.Contains(evt.time))
            {
                var cfg = evt.hitbox;
                if (cfg != null)
                {
                    hitboxSpawner.Spawn(cfg, gameObject, facing, evt.durationOverride);
                }
                hitboxFiredTimes.Add(evt.time);
            }
        }
    }

    private void HandleMotion()
    {
        if (currentVariant.motionEvents == null || rb == null) return;

        foreach (var evt in currentVariant.motionEvents)
        {
            if (elapsed >= evt.time && !motionFiredTimes.Contains(evt.time))
            {
                Vector2 v = evt.velocity;
                if (facing.x < 0f) v.x *= -1f;

                if (evt.applyMode == ActionApplyMode.Set)
                {
                    rb.linearVelocity = v;
                }
                else
                {
                    rb.linearVelocity += v;
                }
                motionFiredTimes.Add(evt.time);
            }
        }
    }

    private void HandleCancelWindow()
    {
        if (currentVariant == null) return;
        if (currentVariant.cancelWindowStart < 0f || currentVariant.cancelWindowEnd < 0f) return;

        cancelWindowOpen = elapsed >= currentVariant.cancelWindowStart && elapsed <= currentVariant.cancelWindowEnd;
    }

    private void EndAction()
    {
        ResumeGravity();
        currentVariant = null;
        elapsed = 0f;
        activeEnd = 0f;
        totalEnd = 0f;
        hitboxFiredTimes.Clear();
        motionFiredTimes.Clear();
        cancelWindowOpen = false;
        currentCancelTags = null;
    }

    private void SuspendGravity(bool zeroVertical)
    {
        if (rb == null) return;
        gravitySuspended = true;
        savedGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        if (zeroVertical)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    private void ResumeGravity()
    {
        if (!gravitySuspended || rb == null) return;
        rb.gravityScale = savedGravityScale;
        gravitySuspended = false;
    }
}
