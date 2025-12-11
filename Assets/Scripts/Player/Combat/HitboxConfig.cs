using UnityEngine;

[CreateAssetMenu(fileName = "HitboxConfig", menuName = "JoyJoey/Hitbox Config")]
public class HitboxConfig : ScriptableObject
{
    public GameObject hitboxPrefab;
    public HitPayload payload;
    public LayerMask targetLayers;
    public bool attachToOwner = true;
    public Vector2 localOffset;
    public float radius = 0.5f;
    public bool flipWithFacing = true;
    public float lifeSeconds = 0.2f;
}
