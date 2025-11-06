using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas

public class MenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
    [Tooltip("El panel con el texto 'Pulsa cualquier tecla'")]
    public GameObject pressKeyPanel;

    [Tooltip("El panel que contiene los botones principales del menú")]
    public GameObject mainMenuPanel;

    [Tooltip("El panel del menú de opciones")]
    public GameObject optionsPanel;

    [Header("Configuración de Escenas")]
    [Tooltip("El nombre de la escena que se cargará al pulsar 'Nueva Partida'")]
    public string firstLevelSceneName = "Level_1";

    void Start()
    {
        // Al empezar, nos aseguramos de que solo el panel inicial esté activo.
        // Time.timeScale = 1f; // Opcional: Asegura que el juego no esté pausado
        pressKeyPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    void Update()
    {
        // Comprobar si el panel inicial está activo y si se ha pulsado cualquier tecla
        if (pressKeyPanel.activeSelf && Input.anyKeyDown)
        {
            ShowMainMenu();
        }
    }

    // Método privado para cambiar del "splash" al menú principal
    private void ShowMainMenu()
    {
        pressKeyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // --- Funciones Públicas para Botones (para enlazar en el Inspector) ---

    public void OnNewGameButton()
    {
        Debug.Log($"Cargando escena: {firstLevelSceneName}");
        // Carga la escena del primer nivel
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void OnContinueButton()
    {
        // Como pediste, por ahora solo un log.
        // Aquí iría la lógica para cargar el último save.
        Debug.Log("Cargando último nivel guardado... (Función no implementada)");
    }

    public void OnChooseLevelButton()
    {
        // Botón deshabilitado para el prototipo
        Debug.Log("Elegir Nivel: (Función no implementada para el prototipo)");
        // No hace nada, como especificaste.
    }

    public void OnOptionsButton()
    {
        // Oculta el menú principal y muestra el de opciones
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnExitGameButton()
    {
        Debug.Log("Saliendo del juego...");

        // Cierra la aplicación
        // Nota: Application.Quit() no funciona en el Editor de Unity,
        // pero funcionará en la build final (PC, consola, móvil).
        Application.Quit();

        // Si quieres forzar que se detenga el playmode en el editor:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // --- Funciones para el Menú de Opciones ---

    // Esta función la deberás asignar a un botón "Atrás" o "Cerrar"
    // que esté DENTRO de tu 'optionsPanel'.
    public void CloseOptions()
    {
        // Oculta opciones y vuelve a mostrar el menú principal
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}