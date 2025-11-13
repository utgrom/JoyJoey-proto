using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los ataques del juego.
/// Hereda de ScriptableObject para poder crear assets de ataques en el editor de Unity.
/// </summary>
public abstract class AtaqueBaseSO : ScriptableObject
{
    [Header("Configuracion General del Ataque")]
    public string nombreAtaque;
    public float dano;
    public string triggerAnimacion;

    [Header("Vinculacion de Input")]
    [Tooltip("Boton que dispara este ataque (J/K/H por defecto).")]
    public ActionKey actionKey = ActionKey.BasicAttack;
    [Tooltip("Si es true, la direccion del input debe coincidir con directionRequirement.")]
    public bool requireSpecificDirection = false;
    public ActionDirection directionRequirement = ActionDirection.Neutral;

    [Header("Estado de Desbloqueo")]
    public bool desbloqueado = false;

    /// <summary>
    /// Metodo abstracto para definir las condiciones en las que el ataque puede ejecutarse.
    /// </summary>
    public abstract bool PuedeEjecutarse(PlayerController controller);

    /// <summary>
    /// Metodo abstracto que contiene la logica de ejecucion del ataque.
    /// </summary>
    public abstract void Execute(PlayerController controller);

    /// <summary>
    /// Devuelve true si este ScriptableObject corresponde al input recibido.
    /// Se puede sobrescribir para realizar validaciones mas complejas (ej. mirar combos).
    /// </summary>
    public virtual bool MatchesInput(ActionKey key, ActionDirection direction)
    {
        if (key != actionKey)
        {
            return false;
        }

        if (!requireSpecificDirection)
        {
            return true;
        }

        return direction == directionRequirement;
    }
}
