using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(BoxCollider2D))]
public class TriggerGizmo2D : MonoBehaviour
{
    public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.25f); // naranja transparente

    private void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Usamos los bounds del collider para dibujar la cajita
        Vector3 center = col.bounds.center;
        Vector3 size   = col.bounds.size;

        // Relleno transparente
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(center, size);

        // Borde más opaco
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}
