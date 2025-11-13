using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public enum PlayerState
{
    Idle,
    Moving,
    Jumping,
    Falling,
    Attacking,
    Dashing
}

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private AbilityManager abilityManager;
    public Animator animator; // Referencia al Animator

    [Header("Attacks")]
    public List<AtaqueBaseSO> listaDeAtaques; // Arrastra aquí tus assets de ataque

    [Header("State")]
    public PlayerState currentState;

    private float prevMoveInput;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        abilityManager = GetComponent<AbilityManager>(); // Can be null
        // Si el Animator está en el mismo objeto, puedes usar GetComponent<Animator>();
    }

    private void Update()
    {
        // --- Input Reading --- //
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);

        bool basicAttackPressed = Input.GetKeyDown(KeyCode.J);
        bool specialAttackPressed = Input.GetKeyDown(KeyCode.K);
        bool trickPressed = Input.GetKeyDown(KeyCode.H);

        bool cycleNextRagPressed = Input.GetKeyDown(KeyCode.E);
        bool cyclePrevRagPressed = Input.GetKeyDown(KeyCode.Q);
        bool transformPressed = Input.GetKeyDown(KeyCode.L);

        // --- State & Direction Calculation --- //
        ActionDirection currentActionDirection = GetActionDirection();

        // --- Delegate to Components --- //

        // Check for wall jump input lock override
        if (prevMoveInput == 0 && moveInput != 0)
        {
            playerMovement.UnlockWallJumpInput();
        }

        // Movement
        playerMovement.SetMoveInput(moveInput);

        // Jump Input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey(KeyCode.S) && playerMovement.IsGrounded)
            {
                playerMovement.DescendPlatform();
            }
            else
            {
                playerMovement.RequestJump();
            }
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            playerMovement.ReleaseJump();
        }

        if (dashPressed)
        {
            HandleDash(moveInput);
        }

        // --- NEW ATTACK LOGIC ---
        if (basicAttackPressed) TryPerformAttack(ActionKey.BasicAttack, currentActionDirection);
        if (specialAttackPressed) TryPerformAttack(ActionKey.SpecialAttack, currentActionDirection);
        if (trickPressed) TryPerformAttack(ActionKey.Trick, currentActionDirection);


        // Combat & Actions (Non-attack related)
        if (cycleNextRagPressed) playerCombat.CycleRag(1);
        if (cyclePrevRagPressed) playerCombat.CycleRag(-1);
        if (transformPressed) playerCombat.ToggleTransformation();

        prevMoveInput = moveInput;
        UpdateState();
    }

    private void UpdateState()
    {
        if (currentState == PlayerState.Attacking || currentState == PlayerState.Dashing) return;

        if (!playerMovement.IsGrounded)
        {
            currentState = playerMovement.GetComponent<Rigidbody2D>().linearVelocity.y > 0 ? PlayerState.Jumping : PlayerState.Falling;
        }
        else
        {
            if (Mathf.Abs(playerMovement.GetComponent<Rigidbody2D>().linearVelocity.x) > 0.1f)
            {
                currentState = PlayerState.Moving;
            }
            else
            {
                currentState = PlayerState.Idle;
            }
        }
    }

    private void TryPerformAttack(ActionKey key, ActionDirection direction)
    {
        if (listaDeAtaques == null || listaDeAtaques.Count == 0)
        {
            return;
        }

        var attackToPerform = listaDeAtaques
            .FirstOrDefault(attack => attack != null && attack.MatchesInput(key, direction));

        if (attackToPerform == null)
        {
            return;
        }

        if (attackToPerform.PuedeEjecutarse(this))
        {
            attackToPerform.Execute(this);
        }
    }

    private ActionDirection GetActionDirection()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            return ActionDirection.Down;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            return ActionDirection.Horizontal;
        }
        return ActionDirection.Neutral;
    }

    private void HandleDash(float moveInput)
    {
        bool canAirDash = abilityManager != null && abilityManager.IsAbilityUnlocked("AirDash");

        if (playerMovement.IsGrounded || canAirDash)
        {
            Vector2 dashDirection = GetActionDirection() switch
            {
                ActionDirection.Down => Vector2.down,
                ActionDirection.Horizontal => new Vector2(Mathf.Sign(moveInput), 0),
                _ => new Vector2(transform.localScale.x, 0) // Default to facing direction
            };

            if (dashDirection.sqrMagnitude < 0.1f)
            {
                 dashDirection = new Vector2(transform.localScale.x, 0); // Fallback if input is neutral
            }

            playerMovement.Dash(dashDirection);
        }
    }

    #region Animation Events
    // Llama a este método desde un Animation Event al final de una animación de ataque.
    public void AnimationEvent_AttackFinished()
    {
        currentState = PlayerState.Idle;
        Debug.Log("Attack Finished. State reset to Idle.");
    }

    // Llama a este método desde un Animation Event en el frame que quieras activar el daño.
    public void AnimationEvent_ActivateHitbox()
    {
        // Aquí iría la lógica para activar una hitbox, por ejemplo:
        // playerCombat.ActivateCurrentAttackHitbox();
        Debug.Log("Hitbox Activated!");
    }

    // Llama a este método desde un Animation Event en el frame que quieras desactivar el daño.
    public void AnimationEvent_DeactivateHitbox()
    {
        // playerCombat.DeactivateCurrentAttackHitbox();
        Debug.Log("Hitbox Deactivated!");
    }
    #endregion
}
