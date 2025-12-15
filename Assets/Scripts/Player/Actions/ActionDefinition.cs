using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionDefinition", menuName = "JoyJoey/Action Definition")]
public class ActionDefinition : ScriptableObject
{
    public string actionId;
    public ActionKey slot;
    public ActionVariant[] variants;
}

[Serializable]
public class ActionVariant
{
    public string variantId;
    public ActionContext context;
    public ActionDirection direction;

    [Header("Timeline (seconds)")]
    public float startup = 0.1f;
    public float active = 0.1f;
    public float recovery = 0.2f;
    public bool canCancelDuringRecovery = false;
    public float cancelWindowStart = -1f;
    public float cancelWindowEnd = -1f;
    public string[] cancelTagsGranted;
    public string[] cancelTagsRequiredToEnter;

    [Header("Motion")]
    public bool zeroVelocityOnStart = true;
    public bool preserveHorizontalMomentum = false;
    public bool lockFacing = true;
    public bool lockMoveKeys = true;
    public GravityMode gravityMode = GravityMode.Normal;
    public ActionMotionEvent[] motionEvents;

    [Header("Hitboxes")]
    public ActionHitboxEvent[] hitboxEvents;

    [Header("Animation")]
    public string animatorTrigger;
}

[Serializable]
public struct ActionMotionEvent
{
    public float time;
    public Vector2 velocity;
    public ActionApplyMode applyMode;
}

[Serializable]
public struct ActionHitboxEvent
{
    public float time;
    public HitboxConfig hitbox;
    public float durationOverride;
}
