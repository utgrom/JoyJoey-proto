using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    [SerializeField] private Transform spriteTransform;

    [Header("Movement Stats")]
    [SerializeField] private float maxMoveSpeed = 10f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float airDeceleration = 5f;

    [Header("Jump Stats")]
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float fallGravityMult = 1.5f;
    [SerializeField] private float jumpCutFactor = 2f;
    [SerializeField] private float defaultGravityScale = 3f;

    [Header("Assists")]
    [SerializeField] private float coyoteTimeDuration = 0.1f;
    [SerializeField] private float jumpBufferDuration = 0.1f;

    [Header("Wall Interaction")]
    [SerializeField] private Vector2 wallCheckOffset;
    [SerializeField] private float wallCheckSphereRadius = 0.2f;
    [SerializeField] private float wallSlideUpwardForce = 5f;
    [SerializeField] private float wallJumpXForce = 10f;
    [SerializeField] private float wallJumpYForce = 18f;
    [SerializeField] private float wallJumpCutFactor = 2f;
    [SerializeField] private float wallJumpCoyoteTime = 0.1f;
    [SerializeField] private float wallJumpBufferTime = 0.1f;
    [SerializeField] private float wallJumpInputLockDuration = 0.2f;
    [SerializeField] private AnimationCurve wallJumpAccelRebuildCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Dash Stats")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashInvincibilityDuration = 0.15f;

    [Header("Ground & Wall Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer; // Used for ground and walls

    // Public State
    public bool IsGrounded { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsWallSliding { get; private set; }
    public bool CanWallJump { get; private set; }

    // Private State
    private float horizontalInput;
    private float dashTimer;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float wallJumpCoyoteTimeCounter;
    private float wallJumpBufferCounter;
    private float wallSide; // -1 for left, 1 for right
    private float wallJumpInputLockTimer;
    private float currentAcceleration;
    private bool isJumpingFromWall = false;
    private bool isFacingRight = true;
    private bool facingLocked = false;
    private bool horizontalControlEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = defaultGravityScale;
        currentAcceleration = acceleration;
    }

    private void Update()
    {
        HandleDashTimer();
        HandleCounters();

        if (jumpBufferCounter > 0f)
        {
            if (wallJumpCoyoteTimeCounter > 0f)
            {
                ExecuteWallJump();
            }
            else if (coyoteTimeCounter > 0f)
            {
                ExecuteJump();
            }
        }
    }

    private void FixedUpdate()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (IsGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
            isJumpingFromWall = false;
        }

        HandleWallInteraction();
        HandleHorizontalMovement();
        HandleGravity();
    }

    private void HandleCounters()
    {
        coyoteTimeCounter -= Time.deltaTime;
        jumpBufferCounter -= Time.deltaTime;
        wallJumpCoyoteTimeCounter -= Time.deltaTime;
        wallJumpBufferCounter -= Time.deltaTime;

        if (wallJumpInputLockTimer > 0)
        {
            wallJumpInputLockTimer -= Time.deltaTime;
            
            if (wallJumpInputLockDuration > 0)
            {
                float curveTime = 1 - (wallJumpInputLockTimer / wallJumpInputLockDuration);
                currentAcceleration = acceleration * wallJumpAccelRebuildCurve.Evaluate(curveTime);
            }

            if (wallJumpInputLockTimer <= 0)
            {
                currentAcceleration = acceleration;
            }
        }
    }

    public void SetMoveInput(float input)
    {
        horizontalInput = input;
    }

    public void RequestJump()
    {
        jumpBufferCounter = jumpBufferDuration;
        wallJumpBufferCounter = wallJumpBufferTime;
    }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        isJumpingFromWall = false;
    }

    private void ExecuteWallJump()
    {
        float jumpDirection = -wallSide;
        rb.linearVelocity = new Vector2(0, 0); // Reset velocity
        rb.AddForce(new Vector2(wallJumpXForce * jumpDirection, wallJumpYForce), ForceMode2D.Impulse);
        
        currentAcceleration = 0f;
        wallJumpInputLockTimer = wallJumpInputLockDuration;

        jumpBufferCounter = 0f;
        wallJumpCoyoteTimeCounter = 0f;
        isJumpingFromWall = true;
    }

    public void ReleaseJump()
    {
        if (rb.linearVelocity.y > 0)
        {
            if (isJumpingFromWall)
            {
                // Cut both horizontal and vertical velocity for wall jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x / wallJumpCutFactor, rb.linearVelocity.y / wallJumpCutFactor);
            }
            else
            {
                // Cut only vertical velocity for ground jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / jumpCutFactor);
            }
        }
    }

    public void DescendPlatform()
    {
        if (!IsGrounded) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -5f);
    }

    private void HandleHorizontalMovement()
    {
        if (IsDashing) return;
        if (!horizontalControlEnabled) return;

        // Flip direction
        if (!facingLocked && horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (!facingLocked && horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }

        float targetSpeed = horizontalInput * maxMoveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? currentAcceleration : (IsGrounded ? deceleration : airDeceleration);

        float movement = speedDif * accelRate;
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Flip(bool force = false)
    {
        if (facingLocked && !force) return;
        isFacingRight = !isFacingRight;
        spriteTransform.localScale = new Vector3(spriteTransform.localScale.x * -1, 1, 1);
    }

    private void HandleWallInteraction()
    {
        Vector2 basePos = (Vector2)transform.position;

        // Right Check
        Vector2 rightCheckPos = basePos + wallCheckOffset;
        Collider2D rightWallHit = Physics2D.OverlapCircle(rightCheckPos, wallCheckSphereRadius, groundLayer);

        // Left Check
        Vector2 leftCheckPos = basePos + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);
        Collider2D leftWallHit = Physics2D.OverlapCircle(leftCheckPos, wallCheckSphereRadius, groundLayer);

        CanWallJump = (rightWallHit != null || leftWallHit != null) && !IsGrounded;

        if (CanWallJump)
        {
            wallSide = rightWallHit != null ? 1 : -1;
            wallJumpCoyoteTimeCounter = wallJumpCoyoteTime;
        }

        bool isPushingWall = (rightWallHit != null && horizontalInput > 0) || (leftWallHit != null && horizontalInput < 0);
        IsWallSliding = CanWallJump && isPushingWall;

        if (IsWallSliding)
        {
            rb.AddForce(Vector2.up * wallSlideUpwardForce, ForceMode2D.Force);
        }
    }

    private void HandleGravity()
    {
        if (IsWallSliding && rb.linearVelocity.y < 0)
        {
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }

        if (rb.linearVelocity.y < 0 && !IsWallSliding)
        {
            rb.gravityScale *= fallGravityMult;
        }
    }

    public void UnlockWallJumpInput()
    {
        if (wallJumpInputLockTimer > 0)
        {
            currentAcceleration = acceleration;
            wallJumpInputLockTimer = 0;
        }
    }

    public void LockFacing(float desiredSign)
    {
        facingLocked = true;
        desiredSign = Mathf.Sign(desiredSign == 0 ? (isFacingRight ? 1f : -1f) : desiredSign);
        if (desiredSign > 0 && !isFacingRight) Flip(force: true);
        else if (desiredSign < 0 && isFacingRight) Flip(force: true);
    }

    public void UnlockFacing()
    {
        facingLocked = false;
    }

    public void SetHorizontalControlEnabled(bool enabled)
    {
        horizontalControlEnabled = enabled;
    }

    private void HandleDashTimer()
    {
        if (IsDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                IsDashing = false;
                rb.gravityScale = defaultGravityScale;
            }
        }
    }

    public void Dash(Vector2 direction)
    {
        if (IsDashing) return;

        IsDashing = true;
        dashTimer = dashDuration;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * dashSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Draw wall check circles
        Gizmos.color = Color.blue;
        Vector2 basePos = (Vector2)transform.position;

        // Right
        Vector2 rightCheckPos = basePos + wallCheckOffset;
        Gizmos.DrawWireSphere(rightCheckPos, wallCheckSphereRadius);

        // Left
        Vector2 leftCheckPos = basePos + new Vector2(-wallCheckOffset.x, wallCheckOffset.y);
        Gizmos.DrawWireSphere(leftCheckPos, wallCheckSphereRadius);
    }
}
