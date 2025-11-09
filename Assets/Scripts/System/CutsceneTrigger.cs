using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject cutsceneObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (cutsceneObject != null)
            {
                cutsceneObject.SetActive(true);
                // Optional: Destroy this trigger so it doesn't run again
                // Destroy(gameObject); 
            }
        }
    }
}
