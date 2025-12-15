using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TriggerGizmo2D : MonoBehaviour
{
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.25f);
    public bool drawFill = true;
    public bool drawWire = true;
    [Range(6, 64)] public int arcSegments = 24;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        var colliders = GetComponents<Collider2D>();
        if (colliders == null || colliders.Length == 0) return;

        foreach (var col in colliders)
        {
            if (!col) continue;

            if (col is BoxCollider2D box) DrawBox(box);
            else if (col is CircleCollider2D circle) DrawCircle(circle);
            else if (col is CapsuleCollider2D capsule) DrawCapsule(capsule);
            else DrawFallbackBounds(col);
        }
#endif
    }

#if UNITY_EDITOR
    private void SetColors(out Color fill, out Color wire)
    {
        fill = gizmoColor;
        wire = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
    }

    private static Vector2 AbsScale(Transform t)
    {
        var s = t.lossyScale;
        return new Vector2(Mathf.Abs(s.x), Mathf.Abs(s.y));
    }

    private void DrawBox(BoxCollider2D box)
    {
        SetColors(out var fill, out var wire);

        var t = box.transform;
        var abs = AbsScale(t);
        var oldMatrix = Handles.matrix;

        Handles.matrix = Matrix4x4.TRS(
            t.TransformPoint(box.offset),
            t.rotation,
            new Vector3(abs.x, abs.y, 1f)
        );

        Vector2 hs = box.size * 0.5f;
        var rect = new Vector3[]
        {
            new Vector3(-hs.x, -hs.y, 0f),
            new Vector3(-hs.x,  hs.y, 0f),
            new Vector3( hs.x,  hs.y, 0f),
            new Vector3( hs.x, -hs.y, 0f),
        };

        if (drawFill || drawWire)
            Handles.DrawSolidRectangleWithOutline(rect, drawFill ? fill : Color.clear, drawWire ? wire : Color.clear);

        Handles.matrix = oldMatrix;
    }

    private void DrawCircle(CircleCollider2D circle)
    {
        SetColors(out var fill, out var wire);

        var t = circle.transform;
        var abs = AbsScale(t);
        var oldMatrix = Handles.matrix;

        // Con matriz escalada, el "disco" se vuelve elipse si scaleX != scaleY (más fiel a 2D)
        Handles.matrix = Matrix4x4.TRS(
            t.TransformPoint(circle.offset),
            t.rotation,
            new Vector3(abs.x, abs.y, 1f)
        );

        if (drawFill)
        {
            Handles.color = fill;
            Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, circle.radius);
        }

        if (drawWire)
        {
            Handles.color = wire;
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, circle.radius);
        }

        Handles.matrix = oldMatrix;
    }

    private void DrawCapsule(CapsuleCollider2D capsule)
    {
        SetColors(out var fill, out var wire);

        var t = capsule.transform;
        var abs = AbsScale(t);
        var oldMatrix = Handles.matrix;

        Handles.matrix = Matrix4x4.TRS(
            t.TransformPoint(capsule.offset),
            t.rotation,
            new Vector3(abs.x, abs.y, 1f)
        );

        var pts = BuildCapsulePolygon(capsule.size, capsule.direction, Mathf.Max(6, arcSegments));

        if (drawFill)
        {
            Handles.color = fill;
            Handles.DrawAAConvexPolygon(pts);
        }

        if (drawWire)
        {
            Handles.color = wire;

            // cerrar línea
            var closed = new Vector3[pts.Length + 1];
            for (int i = 0; i < pts.Length; i++) closed[i] = pts[i];
            closed[pts.Length] = pts[0];

            Handles.DrawAAPolyLine(2f, closed);
        }

        Handles.matrix = oldMatrix;
    }

    private static Vector3[] BuildCapsulePolygon(Vector2 size, CapsuleDirection2D dir, int seg)
    {
        bool vertical = dir == CapsuleDirection2D.Vertical;

        float r = vertical ? (size.x * 0.5f) : (size.y * 0.5f);
        float length = vertical ? size.y : size.x;

        // tramo recto total (sin las tapas)
        float inner = Mathf.Max(0f, length - 2f * r);
        float halfInner = inner * 0.5f;

        // Si no hay tramo recto, se convierte en un "círculo" (disco) escalado por la matriz
        if (inner <= 0.0001f)
        {
            var ptsCircle = new Vector3[seg * 2];
            for (int i = 0; i < ptsCircle.Length; i++)
            {
                float a = (i / (float)ptsCircle.Length) * Mathf.PI * 2f;
                ptsCircle[i] = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
            }
            return ptsCircle;
        }

        var pts = new System.Collections.Generic.List<Vector3>(2 * (seg + 1));

        if (vertical)
        {
            // Top semicircle center (0, +halfInner): angles 0..pi (right -> left)
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.Lerp(0f, Mathf.PI, i / (float)seg);
                pts.Add(new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r + halfInner, 0f));
            }

            // Bottom semicircle center (0, -halfInner): angles pi..2pi (left -> right)
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.Lerp(Mathf.PI, 2f * Mathf.PI, i / (float)seg);
                pts.Add(new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r - halfInner, 0f));
            }
        }
        else
        {
            // Right semicircle center (+halfInner, 0): angles -pi/2..pi/2 (bottom -> top)
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, i / (float)seg);
                pts.Add(new Vector3(Mathf.Cos(a) * r + halfInner, Mathf.Sin(a) * r, 0f));
            }

            // Left semicircle center (-halfInner, 0): angles pi/2..3pi/2 (top -> bottom)
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.Lerp(Mathf.PI * 0.5f, Mathf.PI * 1.5f, i / (float)seg);
                pts.Add(new Vector3(Mathf.Cos(a) * r - halfInner, Mathf.Sin(a) * r, 0f));
            }
        }

        return pts.ToArray();
    }

    private void DrawFallbackBounds(Collider2D col)
    {
        // fallback 2D: dibuja el AABB si es otro collider (Edge/Polygon/Composite, etc.)
        SetColors(out var fill, out var wire);

        var b = col.bounds;
        var oldMatrix = Handles.matrix;
        Handles.matrix = Matrix4x4.identity;

        var rect = new Vector3[]
        {
            new Vector3(b.min.x, b.min.y, 0f),
            new Vector3(b.min.x, b.max.y, 0f),
            new Vector3(b.max.x, b.max.y, 0f),
            new Vector3(b.max.x, b.min.y, 0f),
        };

        Handles.DrawSolidRectangleWithOutline(rect, drawFill ? fill : Color.clear, drawWire ? wire : Color.clear);
        Handles.matrix = oldMatrix;
    }
#endif
}
