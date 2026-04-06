using UnityEngine;

public class Teleporter : MonoBehaviour
{
    private InGameUIManager inGameUIManager;

    void Start()
    {
        // Find the UIManager in the scene
        inGameUIManager = FindAnyObjectByType<InGameUIManager>();

        if (inGameUIManager == null)
        {
            Debug.LogError("Teleporter could not find InGameUIManager in the scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (inGameUIManager == null) return;

        // Check if it's the Player and the game is in the Idle state
        if (other.CompareTag("Player") && inGameUIManager.GetCurrentState() == InGameUIManager.InGameState.Idle)
        {
            Debug.Log("Player entered Teleporter, showing Travel Panel.");
            // Request the UIManager to show the panel
            inGameUIManager.ShowTravelPanel();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (inGameUIManager == null) return;

        // Check if it's the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited Teleporter, hiding Travel Panel.");
            // Request the UIManager to hide the panel
            inGameUIManager.HideTravelPanel();
        }
    }
}