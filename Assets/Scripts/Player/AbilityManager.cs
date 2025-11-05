
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the abilities the player has unlocked.
/// </summary>
public class AbilityManager : MonoBehaviour
{
    // A simple list of strings to identify unlocked abilities.
    // e.g., "AirDash", "Rag.Strongman.Special.Down"
    [SerializeField]
    private List<string> unlockedAbilities = new List<string>();

    public bool IsAbilityUnlocked(string abilityID)
    {
        if (string.IsNullOrEmpty(abilityID))
        {
            return true; // No specific ability required
        }
        return unlockedAbilities.Contains(abilityID);
    }

    public void UnlockAbility(string abilityID)
    {
        if (!string.IsNullOrEmpty(abilityID) && !unlockedAbilities.Contains(abilityID))
        {
            unlockedAbilities.Add(abilityID);
        }
    }
}
