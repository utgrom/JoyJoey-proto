using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputRouter : MonoBehaviour
{
    [Header("Input Asset")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Gameplay";

    public event Action<Vector2> OnMove;
    public event Action OnJumpStarted;
    public event Action OnJumpCanceled;
    public event Action OnDash;
    public event Action<ActionKey, ActionDirection> OnActionKey;
    public event Action OnRagNext;
    public event Action OnRagPrev;

    private InputAction move;
    private InputAction jump;
    private InputAction dash;
    private InputAction actionBasic;
    private InputAction actionSpecial;
    private InputAction actionTrick;
    private InputAction ragNext;
    private InputAction ragPrev;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            Debug.LogError("PlayerInputRouter: missing InputActionAsset.");
            return;
        }

        var map = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
        if (map == null)
        {
            Debug.LogError($"PlayerInputRouter: action map '{actionMapName}' not found.");
            return;
        }

        move = map.FindAction("Move");
        jump = map.FindAction("Jump");
        dash = map.FindAction("Dash");
        actionBasic = map.FindAction("ActionBasic");
        actionSpecial = map.FindAction("ActionSpecial");
        actionTrick = map.FindAction("ActionTrick");
        ragNext = map.FindAction("RagNext");
        ragPrev = map.FindAction("RagPrev");

        map.Enable();

        if (move != null) move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        if (move != null) move.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        if (jump != null) jump.started += _ => OnJumpStarted?.Invoke();
        if (jump != null) jump.canceled += _ => OnJumpCanceled?.Invoke();

        if (dash != null) dash.started += _ => OnDash?.Invoke();

        if (actionBasic != null) actionBasic.started += _ => DispatchAction(ActionKey.Basic);
        if (actionSpecial != null) actionSpecial.started += _ => DispatchAction(ActionKey.Special);
        if (actionTrick != null) actionTrick.started += _ => DispatchAction(ActionKey.Trick);

        if (ragNext != null) ragNext.started += _ => OnRagNext?.Invoke();
        if (ragPrev != null) ragPrev.started += _ => OnRagPrev?.Invoke();
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        var map = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
        map?.Disable();
    }

    private void DispatchAction(ActionKey key)
    {
        Vector2 moveInput = move != null ? move.ReadValue<Vector2>() : Vector2.zero;
        ActionDirection dir = ResolveDirection(moveInput);
        OnActionKey?.Invoke(key, dir);
    }

    private ActionDirection ResolveDirection(Vector2 moveInput)
    {
        if (moveInput.y < -0.5f)
            return ActionDirection.Down;
        if (Mathf.Abs(moveInput.x) > 0.1f)
            return ActionDirection.Horizontal;
        return ActionDirection.Neutral;
    }
}
