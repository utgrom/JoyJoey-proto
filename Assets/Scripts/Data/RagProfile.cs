
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Rag", menuName = "JoyJoey/Rag Profile")]
public class RagProfile : ScriptableObject
{
    [Header("Rag Info")]
    public string RagName = "New Rag";
    public string RagId; // Unique ID for this rag

    [Header("Costume Transformation")]
    public bool CanTransform = true;
    public float FunCostPerSecond = 10f;
    public float HealthRegenPerSecond = 5f;
    public float DamageMultiplier = 1.5f;

    [Header("Actions")]
    public ActionSet SpecialAttacks; // K key
    public ActionSet Tricks; // H key
    public ActionSet CostumeAttacks; // J key while transformed

    /// <summary>
    /// Finds the correct action based on the player's state and input direction.
    /// </summary>
    public ActionDefinition GetAction(ActionSet set, ActionState state, ActionDirection direction)
    {
        ActionDefinition action = null;

        if (state == ActionState.Ground)
        {
            action = GetDirectionalAction(set.GroundActions, direction);
        }
        else // Air
        {
            action = GetDirectionalAction(set.AirActions, direction);
        }

        return action;
    }

    private ActionDefinition GetDirectionalAction(DirectionalActionSet directionalSet, ActionDirection direction)
    {
        ActionDefinition action = null;
        switch (direction)
        {
            case ActionDirection.Neutral:
                action = directionalSet.Neutral;
                break;
            case ActionDirection.Horizontal:
                action = directionalSet.Horizontal;
                break;
            case ActionDirection.Down:
                action = directionalSet.Down;
                break;
        }

        // Fallback logic as per the guide
        if (action == null)
        {
            if (direction == ActionDirection.Neutral)
                return directionalSet.Horizontal; // Try horizontal if neutral is missing
            if (direction == ActionDirection.Horizontal)
                return directionalSet.Neutral; // Try neutral if horizontal is missing
        }

        return action;
    }
}

/// <summary>
/// A helper class to organize actions for a specific button.
/// </summary>
[Serializable]
public class ActionSet
{
    public DirectionalActionSet GroundActions;
    public DirectionalActionSet AirActions;
}

/// <summary>
/// A helper class to organize actions for different directions in a given state (ground or air).
/// </summary>
[Serializable]
public class DirectionalActionSet
{
    public ActionDefinition Neutral;
    public ActionDefinition Horizontal;
    public ActionDefinition Down;
}
