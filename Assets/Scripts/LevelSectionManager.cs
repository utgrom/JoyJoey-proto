using UnityEngine;
using Unity.Cinemachine;   //  importante en Unity 6 + Cinemachine 3

public class LevelSectionManager : MonoBehaviour
{
    [System.Serializable]
    public class Section
    {
        public string name;
        public Transform playerSpawn;
        public Collider2D cameraBounds;
    }

    [Header("Referencias")]
    public Transform player;
    public Rigidbody2D playerRb;

    // Puedes seguir usando CinemachineVirtualCamera (está deprecado pero funciona)
    // o cambiarlo a CinemachineCamera si tu componente nuevo se llama así.
    public CinemachineVirtualCamera mainVcam;
    public CinemachineConfiner2D confiner;

    [Header("Secciones del nivel (en orden)")]
    public Section[] sections;

    int currentIndex = 0;

    void Awake()
    {
        // Si no asignaste el confiner a mano, intenta buscarlo en la cámara
        if (confiner == null && mainVcam != null)
        {
            confiner = mainVcam.GetComponent<CinemachineConfiner2D>();
        }
    }

    /// <summary>
    /// Teletransporta jugador y cámara a una sección concreta (para usar desde Timeline / Signal).
    /// </summary>
    public void TeleportToSection(int index)
    {
        if (index < 0 || index >= sections.Length)
        {
            Debug.LogWarning($"LevelSectionManager: índice de sección inválido ({index}).");
            return;
        }

        currentIndex = index;
        var section = sections[index];

        // Frenar al jugador para que no arrastre velocidad al teleporte
        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        // Mover jugador al spawn de esta sección
        if (player != null && section.playerSpawn != null)
            player.position = section.playerSpawn.position;

        // Cambiar bounds de la cámara
        if (confiner != null && section.cameraBounds != null)
        {
            confiner.BoundingShape2D = section.cameraBounds;
            confiner.InvalidateCache(); // forza recalcular el confiner con la nueva forma
        }
    }

    /// <summary>
    /// Atajo para avanzar a la próxima sección (ideal para un Signal de Timeline).
    /// </summary>
    public void TeleportToNextSection()
    {
        TeleportToSection(currentIndex + 1);
    }
}
