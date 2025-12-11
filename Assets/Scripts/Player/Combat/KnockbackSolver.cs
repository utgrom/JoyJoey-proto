using UnityEngine;

public static class KnockbackSolver
{
    public static Vector2 Solve(HitContext context, float weightHorizontal, float weightVertical)
    {
        Vector2 kb = context.payload.knockback;

        if (context.facing.x < 0f)
        {
            kb.x *= -1f;
        }

        float x = kb.x / Mathf.Max(0.01f, weightHorizontal);
        float y = kb.y / Mathf.Max(0.01f, weightVertical);

        if (context.payload.inheritSourceVerticalVelocity)
        {
            y = context.sourceVelocity.y;
        }

        return new Vector2(x, y);
    }
}
