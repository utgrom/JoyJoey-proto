using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

public class AnimatorBoolBinder : MonoBehaviour
{
    [System.Serializable]
    public class BoolBinding
    {
        [Tooltip("Componente fuente (por ej. PlayerMovement, AttackController, etc.)")]
        public Component source;

        [Tooltip("Nombre del campo o propiedad bool en 'source' (por ej. IsWallSliding)")]
        public string memberName = "IsSomething";

        [Tooltip("Nombre del parámetro Bool en el Animator")]
        public string animatorBool = "Param";

        [Tooltip("Invierte el valor antes de mandarlo al Animator")]
        public bool invert;

        // Cache interno (no tocar)
        [HideInInspector] public FieldInfo cachedField;
        [HideInInspector] public PropertyInfo cachedProp;
    }

    [Header("Refs")]
    [SerializeField] private Animator animator;

    [Header("Bindings")]
    [SerializeField] private List<BoolBinding> boolBindings = new List<BoolBinding>();

    [Header("Opcional")]
    [SerializeField] private bool warnIfAnimatorBoolMissing = true;

    private void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        CacheMembers();
        if (warnIfAnimatorBoolMissing) WarnMissingAnimatorBools();
    }

    private void Update()
    {
        if (!animator) return;

        foreach (var b in boolBindings)
        {
            if (b.source == null || string.IsNullOrEmpty(b.memberName) || string.IsNullOrEmpty(b.animatorBool))
                continue;

            if (TryGetBool(b, out bool value))
            {
                animator.SetBool(b.animatorBool, b.invert ? !value : value);
            }
        }
    }

    // --- Helpers ---

    private void CacheMembers()
    {
        foreach (var b in boolBindings)
        {
            if (b.source == null || string.IsNullOrEmpty(b.memberName)) continue;

            var t = b.source.GetType();

            // Campo
            b.cachedField = t.GetField(b.memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Propiedad (solo lectura basta)
            if (b.cachedField == null)
            {
                b.cachedProp = t.GetProperty(b.memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }
    }

    private bool TryGetBool(BoolBinding b, out bool value)
    {
        value = false;

        // Campo
        if (b.cachedField != null && b.cachedField.FieldType == typeof(bool))
        {
            value = (bool)b.cachedField.GetValue(b.source);
            return true;
        }

        // Propiedad
        if (b.cachedProp != null && b.cachedProp.PropertyType == typeof(bool) && b.cachedProp.GetGetMethod(true) != null)
        {
            value = (bool)b.cachedProp.GetValue(b.source, null);
            return true;
        }

        return false;
    }

    private void WarnMissingAnimatorBools()
    {
        if (!animator) return;

        var parameters = animator.parameters;
        foreach (var b in boolBindings)
        {
            bool found = false;
            foreach (var p in parameters)
            {
                if (p.type == AnimatorControllerParameterType.Bool && p.name == b.animatorBool)
                {
                    found = true; break;
                }
            }
            if (!found && !string.IsNullOrEmpty(b.animatorBool))
            {
                Debug.LogWarning($"[AnimatorBoolBinder] Falta el parámetro Bool '{b.animatorBool}' en el Animator.", this);
            }
        }
    }
}
