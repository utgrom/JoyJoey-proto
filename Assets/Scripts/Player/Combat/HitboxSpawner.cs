using UnityEngine;

public class HitboxSpawner : MonoBehaviour
{
    public HitboxInstance Spawn(HitboxConfig config, GameObject owner, Vector2 facing, float lifeOverride = -1f)
    {
        if (config == null || config.hitboxPrefab == null)
        {
            Debug.LogWarning("HitboxSpawner: missing config or prefab.");
            return null;
        }

        var go = Instantiate(config.hitboxPrefab, owner.transform.position, Quaternion.identity);
        var projectile = go.GetComponent<ProjectileMover>();
        if (projectile != null)
        {
            projectile.Init(owner, facing);
        }

        var instance = go.GetComponent<HitboxInstance>();
        if (instance == null)
        {
            instance = go.AddComponent<HitboxInstance>();
        }

        if (lifeOverride > 0f)
        {
            config = Instantiate(config);
            config.lifeSeconds = lifeOverride;
        }

        instance.Init(config, owner, facing);
        return instance;
    }
}
