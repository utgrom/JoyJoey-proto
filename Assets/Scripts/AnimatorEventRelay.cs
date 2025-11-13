using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorEventRelay : MonoBehaviour
{
    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponentInParent<PlayerController>();
        if (controller == null)
            Debug.LogWarning("AnimatorEventRelay: No se encontró PlayerController en padres.");
    }

    public void AnimationEvent_AttackFinished()   { controller?.AnimationEvent_AttackFinished(); }
    public void AnimationEvent_ActivateHitbox()   { controller?.AnimationEvent_ActivateHitbox(); }
    public void AnimationEvent_DeactivateHitbox() { controller?.AnimationEvent_DeactivateHitbox(); }
}
