
using UnityEngine;

/// <summary>
/// Defines a single action, like an attack or a trick.
/// This is a ScriptableObject, so you can create instances of it in the editor.
/// </summary>
[CreateAssetMenu(fileName = "ActionDef", menuName = "JoyJoey/Action Definition")]
public class ActionDefinition : ScriptableObject
{
    [Header("Action Properties")]
    public string ActionName = "New Action";
    // In a real implementation, you would have fields for:
    // public AnimationClip animationClip;
    // public float damage = 10f;
    // public GameObject hitboxPrefab;
    // public float actionDuration = 0.5f;
    // etc.

    [Tooltip("A simple description of what this action does.")]
    [TextArea]
    public string Description = "This is a template action.";
}
