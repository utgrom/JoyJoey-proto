using UnityEngine;

public class CutsceneTimeController : MonoBehaviour
{
    [Header("Opcional: scripts de gameplay a desactivar durante la cinemática")]
    public MonoBehaviour[] scriptsToDisable; 
    // Ej: tu script de movimiento del jugador, spawners, IA, etc.

    private float previousTimeScale = 1f;

    public void PauseGame()
    {
        // Guardar el timeScale actual y ponerlo en 0
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Desactivar scripts de gameplay (opcional pero recomendable)
        foreach (var s in scriptsToDisable)
        {
            if (s != null)
                s.enabled = false;
        }

        Debug.Log("CutsceneTimeController: juego pausado (timeScale = 0).");
    }

    public void ResumeGame()
    {
        // Restaurar timeScale
        Time.timeScale = previousTimeScale;

        // Reactivar scripts de gameplay
        foreach (var s in scriptsToDisable)
        {
            if (s != null)
                s.enabled = true;
        }

        Debug.Log("CutsceneTimeController: juego reanudado (timeScale restaurado).");
    }
}
