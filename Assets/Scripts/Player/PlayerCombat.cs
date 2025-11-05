
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerStats), typeof(AbilityManager))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Components")]
    private PlayerStats playerStats;
    private AbilityManager abilityManager;

    [Header("Rag Inventory")]
    [SerializeField] private List<RagProfile> collectedRags = new List<RagProfile>();
    private int currentRagIndex = -1;
    public RagProfile CurrentRag { get; private set; }

    [Header("Basic Attacks")]
    [SerializeField] private ActionSet basicAttacks; // J key

    [Header("Transformation State")]
    public bool IsTransformed { get; private set; }

    // Event for UI or other systems
    public event System.Action<RagProfile> OnRagChanged;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        abilityManager = GetComponent<AbilityManager>();

        if (collectedRags.Count > 0)
        {
            currentRagIndex = 0;
            CurrentRag = collectedRags[currentRagIndex];
        }
    }

    private void Update()
    {
        if (IsTransformed)
        {
            HandleTransformation();
        }
    }

    /// <summary>
    /// Rotates the Rag selection.
    /// </summary>
    public void CycleRag(int direction)
    {
        if (IsTransformed || collectedRags.Count <= 1)
        {
            return;
        }

        currentRagIndex += direction;

        // Wrap around the list
        if (currentRagIndex < 0) currentRagIndex = collectedRags.Count - 1;
        if (currentRagIndex >= collectedRags.Count) currentRagIndex = 0;

        CurrentRag = collectedRags[currentRagIndex];
        OnRagChanged?.Invoke(CurrentRag);
        Debug.Log($"Switched to Rag: {CurrentRag.RagName}");
    }

    /// <summary>
    /// Attempts to perform an action based on the input button and player state.
    /// </summary>
    public void TryPerformAction(ActionKey actionKey, ActionState state, ActionDirection direction)
    {
        ActionDefinition actionToPerform = null;

        if (IsTransformed)
        {
            // While transformed, J becomes Costume Attacks
            if (actionKey == ActionKey.BasicAttack)
            {
                actionToPerform = CurrentRag?.GetAction(CurrentRag.CostumeAttacks, state, direction);
            }
            else // K and H are the Rag's regular special/trick attacks
            {
                actionToPerform = GetRagAction(actionKey, state, direction);
            }
        }
        else // Not transformed
        {
            if (actionKey == ActionKey.BasicAttack)
            {
                actionToPerform = CurrentRag?.GetAction(basicAttacks, state, direction);
            }
            else // K and H are the Rag's attacks
            {
                actionToPerform = GetRagAction(actionKey, state, direction);
            }
        }

        if (actionToPerform != null)
        {
            ExecuteAction(actionToPerform);
        }
        else
        {
            Debug.LogWarning($"No action found for {actionKey} in state {state} with direction {direction}");
        }
    }

    private ActionDefinition GetRagAction(ActionKey key, ActionState state, ActionDirection direction)
    {
        if (CurrentRag == null) return null;

        ActionSet actionSet = null;
        if (key == ActionKey.SpecialAttack) actionSet = CurrentRag.SpecialAttacks;
        if (key == ActionKey.Trick) actionSet = CurrentRag.Tricks;

        return CurrentRag.GetAction(actionSet, state, direction);
    }

    private void ExecuteAction(ActionDefinition action)
    {
        // In a real game, this would trigger animations, create hitboxes, etc.
        Debug.Log($"Performing Action: {action.ActionName}");

        // After dealing damage, you would add FUN to the player
        // playerStats.AddFun(damageDealt * funMultiplier);
    }

    /// <summary>
    // Toggles the Costume transformation.
    /// </summary>
    public void ToggleTransformation()
    {
        if (IsTransformed)
        {
            DeactivateTransformation();
            return;
        }

        // Conditions to transform
        if (CurrentRag != null && CurrentRag.CanTransform && playerStats.CurrentFun > 25f) // Example: require 25 FUN to start
        {
            ActivateTransformation();
        }
    }

    private void ActivateTransformation()
    {
        IsTransformed = true;
        Debug.Log($"Transformed into {CurrentRag.RagName}!");
        // TODO: Apply stat boosts, start health regen
    }

    private void DeactivateTransformation()
    {
        IsTransformed = false;
        Debug.Log("Transformation ended.");
        // TODO: Remove stat boosts, stop health regen
    }

    private void HandleTransformation()
    {
        if (CurrentRag == null) 
        {
            DeactivateTransformation();
            return;
        }

        // Use FUN over time
        bool hasFun = playerStats.TryUseFun(CurrentRag.FunCostPerSecond * Time.deltaTime);
        if (!hasFun)
        {
            DeactivateTransformation();
            return;
        }

        // Regenerate health
        playerStats.Heal(CurrentRag.HealthRegenPerSecond * Time.deltaTime);
    }
}

public enum ActionKey { BasicAttack, SpecialAttack, Trick }
