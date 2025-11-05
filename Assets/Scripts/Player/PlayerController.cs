using UnityEngine;

[RequireComponent(typeof(PlayerMovement), typeof(PlayerCombat))]
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private AbilityManager abilityManager;

    private float prevMoveInput;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        abilityManager = GetComponent<AbilityManager>(); // Can be null
    }

    private void Update()
    {
        // --- Input Reading --- //
        float moveInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);

        bool basicAttackPressed = Input.GetKeyDown(KeyCode.J);
        bool specialAttackPressed = Input.GetKeyDown(KeyCode.K);
        bool trickPressed = Input.GetKeyDown(KeyCode.H);

        bool cycleNextRagPressed = Input.GetKeyDown(KeyCode.E);
        bool cyclePrevRagPressed = Input.GetKeyDown(KeyCode.Q);
        bool transformPressed = Input.GetKeyDown(KeyCode.L);

        // --- State & Direction Calculation --- //
        ActionState currentActionState = playerMovement.IsGrounded ? ActionState.Ground : ActionState.Air;
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

        // Combat & Actions
        if (cycleNextRagPressed) playerCombat.CycleRag(1);
        if (cyclePrevRagPressed) playerCombat.CycleRag(-1);
        if (transformPressed) playerCombat.ToggleTransformation();

        if (basicAttackPressed) playerCombat.TryPerformAction(ActionKey.BasicAttack, currentActionState, currentActionDirection);
        if (specialAttackPressed) playerCombat.TryPerformAction(ActionKey.SpecialAttack, currentActionState, currentActionDirection);
        if (trickPressed) playerCombat.TryPerformAction(ActionKey.Trick, currentActionState, currentActionDirection);

        prevMoveInput = moveInput;
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
}
