using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerActionRunner))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInputRouter inputRouter;
    [SerializeField] private PlayerActionRunner actionRunner;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private RagInventory ragInventory;
    [SerializeField] private Health health;
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private ActionDefinition basicActions;

    [Header("State")]
    [SerializeField] private PlayerState currentState;
    [SerializeField] private float hitstunTimer;

    private ActionResolver actionResolver;
    private float lastFacing = 1f;
    private Vector2 lastMoveInput;
    private bool facingLockedDuringAction;
    private bool moveKeysLocked;

    private void Awake()
    {
        if (!inputRouter) inputRouter = GetComponent<PlayerInputRouter>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!actionRunner) actionRunner = GetComponent<PlayerActionRunner>();
        if (!ragInventory) ragInventory = GetComponent<RagInventory>();
        if (!health) health = GetComponent<Health>();
        if (!hurtbox) hurtbox = GetComponent<Hurtbox>();

        actionResolver = new ActionResolver(basicActions, ragInventory);
    }

    private void OnEnable()
    {
        if (inputRouter != null)
        {
            inputRouter.OnMove += HandleMove;
            inputRouter.OnJumpStarted += HandleJumpStart;
            inputRouter.OnJumpCanceled += HandleJumpCancel;
            inputRouter.OnDash += HandleDash;
            inputRouter.OnActionKey += HandleAction;
            inputRouter.OnRagNext += () => ragInventory?.RotateNext();
            inputRouter.OnRagPrev += () => ragInventory?.RotatePrev();
        }

        if (hurtbox != null)
        {
            hurtbox.OnHitApplied += HandleHit;
        }
    }

    private void OnDisable()
    {
        if (inputRouter != null)
        {
            inputRouter.OnMove -= HandleMove;
            inputRouter.OnJumpStarted -= HandleJumpStart;
            inputRouter.OnJumpCanceled -= HandleJumpCancel;
            inputRouter.OnDash -= HandleDash;
            inputRouter.OnActionKey -= HandleAction;
        }

        if (hurtbox != null)
        {
            hurtbox.OnHitApplied -= HandleHit;
        }
    }

    private void Update()
    {
        if (health != null && health.IsDead)
        {
            currentState = PlayerState.Dead;
            return;
        }

        if (currentState == PlayerState.Hitstun)
        {
            hitstunTimer -= Time.deltaTime;
            if (hitstunTimer <= 0f)
            {
                currentState = movement.IsGrounded ? PlayerState.IdleGround : PlayerState.IdleAir;
            }
            return;
        }

        if (actionRunner.IsPlaying)
        {
            currentState = PlayerState.Action;
        }
        else if (movement.IsDashing)
        {
            currentState = PlayerState.Dash;
        }
        else if (movement.IsWallSliding)
        {
            currentState = PlayerState.WallSlide;
        }
        else
        {
            currentState = movement.IsGrounded ? PlayerState.IdleGround : PlayerState.IdleAir;
        }
    }

    private void HandleMove(Vector2 moveInput)
    {
        if (currentState == PlayerState.Dead) return;
        lastMoveInput = moveInput;

        float appliedMove = moveKeysLocked ? 0f : moveInput.x;
        movement.SetMoveInput(appliedMove);

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            lastFacing = Mathf.Sign(moveInput.x);
        }
    }

    private void HandleJumpStart()
    {
        if (currentState == PlayerState.Dead) return;
        bool downHeld = lastMoveInput.y < -0.5f;

        if (moveKeysLocked) return;

        if (downHeld && movement.IsGrounded)
        {
            movement.DescendPlatform();
        }
        else
        {
            movement.RequestJump();
        }
    }

    private void HandleJumpCancel()
    {
        if (currentState == PlayerState.Dead) return;
        movement.ReleaseJump();
    }

    private void HandleDash()
    {
        if (currentState == PlayerState.Dead) return;
        if (!movement.IsGrounded) return;

        Vector2 dir = new(lastFacing, 0f);
        movement.Dash(dir);
        currentState = PlayerState.Dash;
    }

    private void HandleAction(ActionKey key, ActionDirection direction)
    {
        if (currentState == PlayerState.Dead) return;

        ActionContext ctx = movement.IsGrounded ? ActionContext.Ground : ActionContext.Air;
        var variant = actionResolver.Resolve(key, direction, ctx);
        if (variant == null) return;

        if (actionRunner.CanEnter(variant))
        {
            if (facingLockedDuringAction && !variant.lockFacing)
            {
                movement.UnlockFacing();
                facingLockedDuringAction = false;
            }

            Vector2 facingDir = new(lastFacing, 0f);
            actionRunner.TryStart(variant, facingDir);
            if (variant.lockFacing)
            {
                movement.LockFacing(facingDir.x);
                facingLockedDuringAction = true;
            }
            moveKeysLocked = variant.lockMoveKeys;
            currentState = PlayerState.Action;
        }
    }

    private void HandleHit(HitContext ctx)
    {
        if (currentState == PlayerState.Dead) return;

        if (ctx.payload.cancelPlayerAction || !actionRunner.IsPlaying)
        {
            actionRunner.ForceCancel();
            hitstunTimer = ctx.payload.hitstunSeconds;
            currentState = PlayerState.Hitstun;
            moveKeysLocked = false;
            if (facingLockedDuringAction)
            {
                movement.UnlockFacing();
                facingLockedDuringAction = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (!actionRunner.IsPlaying && facingLockedDuringAction)
        {
            movement.UnlockFacing();
            facingLockedDuringAction = false;
        }

        if (!actionRunner.IsPlaying || actionRunner.CurrentVariant == null || !actionRunner.BeforeRecoveryPhase)
        {
            moveKeysLocked = false;
        }
    }
}
