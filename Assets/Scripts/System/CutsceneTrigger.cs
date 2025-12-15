using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneObject;
    [SerializeField] private bool interactableOnly = false;
    [SerializeField] private GameObject secondaryObject;

    [Header("Input (Gameplay/Interact)")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private string actionMapName = "Gameplay";
    [SerializeField] private string interactActionName = "Interact";

    private InputAction interactAction;
    private bool playerInside;

    private void OnEnable()
    {
        SetupInteractAction();

        if (interactableOnly && secondaryObject != null)
        {
            secondaryObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
            interactAction = null;
        }
    }

    private void SetupInteractAction()
    {
        if (!interactableOnly)
            return;

        if (inputActions == null)
        {
            Debug.LogWarning("CutsceneTrigger: missing InputActionAsset reference.");
            return;
        }

        var map = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
        if (map == null)
        {
            Debug.LogWarning($"CutsceneTrigger: action map '{actionMapName}' not found.");
            return;
        }

        interactAction = map.FindAction(interactActionName, throwIfNotFound: false);
        if (interactAction == null)
        {
            Debug.LogWarning($"CutsceneTrigger: action '{interactActionName}' not found in map '{actionMapName}'.");
            return;
        }

        interactAction.performed -= OnInteract;
        interactAction.performed += OnInteract;

        if (!interactAction.enabled)
        {
            interactAction.Enable();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!interactableOnly)
        {
            ActivateCutscene();
            return;
        }

        playerInside = true;
        if (secondaryObject != null)
        {
            secondaryObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !interactableOnly)
            return;

        playerInside = false;
        if (secondaryObject != null)
        {
            secondaryObject.SetActive(false);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!playerInside)
            return;

        ActivateCutscene();
    }

    private void ActivateCutscene()
    {
        if (cutsceneObject != null)
        {
            cutsceneObject.SetActive(true);
        }
    }
}
