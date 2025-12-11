using UnityEngine;

/// <summary>
/// Data describing how a hit should affect a target.
/// </summary>
[System.Serializable]
public struct HitPayload
{
    public float damage;
    public float hitstunSeconds;
    public float armorBreak;
    public Vector2 knockback;
    public bool resetVerticalVelocity;
    public bool inheritSourceVerticalVelocity;
    public bool ignoreIFrames;
    public bool cancelPlayerAction;
    public bool launchesAirborneUpward;
}

public struct HitContext
{
    public HitPayload payload;
    public GameObject source;
    public Vector2 sourcePosition;
    public Vector2 sourceVelocity;
    public Vector2 facing; // normalized
}

public interface IDamageable
{
    void ApplyHit(HitContext context);
}
