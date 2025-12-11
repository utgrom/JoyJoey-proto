using UnityEngine;

public enum PlayerState
{
    IdleGround,
    IdleAir,
    Moving,
    Jumping,
    Falling,
    Dash,
    WallSlide,
    Action,
    Hitstun,
    Dead
}

[RequireComponent(typeof(PlayerMovement))]
public class PlayerController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    [Header("State")]
    public PlayerState currentState;

    private float prevMoveInput;
    private float lastFacing = 1f;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
        bool jumpReleased = Input.GetKeyUp(KeyCode.Space);
        bool downHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            lastFacing = Mathf.Sign(moveInput);
        }

        if (prevMoveInput == 0 && moveInput != 0)
        {
            playerMovement.UnlockWallJumpInput();
        }

        playerMovement.SetMoveInput(moveInput);

        if (jumpPressed)
        {
            if (downHeld && playerMovement.IsGrounded)
            {
                playerMovement.DescendPlatform();
            }
            else
            {
                playerMovement.RequestJump();
            }
        }

        if (jumpReleased)
        {
            playerMovement.ReleaseJump();
        }

        if (dashPressed)
        {
            HandleDash(downHeld);
        }

        prevMoveInput = moveInput;
        UpdateState();
    }

    private void HandleDash(bool downHeld)
    {
        if (!playerMovement.IsGrounded) return;

        Vector2 dashDirection = downHeld ? Vector2.down : new Vector2(lastFacing, 0f);

        playerMovement.Dash(dashDirection);
        currentState = PlayerState.Dash;
    }

    private void UpdateState()
    {
        if (playerMovement.IsDashing)
        {
            currentState = PlayerState.Dash;
            return;
        }

        if (!playerMovement.IsGrounded)
        {
            currentState = rb.linearVelocity.y > 0 ? PlayerState.Jumping : PlayerState.Falling;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            currentState = PlayerState.Moving;
        }
        else
        {
            currentState = PlayerState.IdleGround;
        }
    }
}

