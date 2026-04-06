using UnityEngine;


[RequireComponent(typeof(FishingController))]
public class PlayerInteraction : MonoBehaviour
{
    private FishingController fishingController;

    void Start()
    {
        
        fishingController = GetComponent<FishingController>();
        if (fishingController == null)
        {
            Debug.LogError("PlayerInteraction could not find FishingController on the same GameObject!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        FishingSpot spot = other.GetComponent<FishingSpot>();
        if (spot != null && fishingController != null)
        {
            
            fishingController.EnterFishingSpot(spot);
        }

        
        
    }

    private void OnTriggerExit(Collider other)
    {
        
        FishingSpot spot = other.GetComponent<FishingSpot>();
        if (spot != null && fishingController != null)
        {
            
            fishingController.ExitFishingSpot();
        }

        
    }
}