using UnityEngine;

/// <summary>
/// Ejemplo de ataque especifico que muestra como usar el sistema basado en ScriptableObjects.
/// </summary>
[CreateAssetMenu(fileName = "PatadaFuerte", menuName = "Ataques/Patada Fuerte")]
public class PatadaFuerteSO : AtaqueBaseSO
{
    /// <summary>
    /// Define si la patada fuerte se puede ejecutar.
    /// Regla basica: debe estar desbloqueado, el jugador no puede estar atacando y debe estar en el suelo.
    /// </summary>
    public override bool PuedeEjecutarse(PlayerController controller)
    {
        bool isGrounded = controller.GetComponent<PlayerMovement>().IsGrounded;
        return desbloqueado && controller.currentState != PlayerState.Attacking && isGrounded;
    }

    /// <summary>
    /// Ejecuta la logica principal de la patada.
    /// </summary>
    public override void Execute(PlayerController controller)
    {
        controller.currentState = PlayerState.Attacking;

        if (controller.animator != null && !string.IsNullOrEmpty(triggerAnimacion))
        {
            controller.animator.SetTrigger(triggerAnimacion);
        }

        Debug.Log($"Ejecutando {nombreAtaque} con {dano} de dano. Trigger de animacion: {triggerAnimacion}");
    }
}
