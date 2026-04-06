using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPickup : MonoBehaviour
{
    private FishInstance fishInstance;
    private PlayerInventorySO playerInventory;

    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("[FishPickup] Không tìm thấy AudioManager.instance!");
        }
    }

    public void Initialize(FishInstance instance, PlayerInventorySO inventory)
    {
        this.fishInstance = instance;
        this.playerInventory = inventory;

        float weightRange = instance.baseData.maxWeight - instance.baseData.minWeight;
        if (weightRange <= 0) weightRange = 1; 
        float weightRatio = (instance.actualWeight - instance.baseData.minWeight) / weightRange;

        float scaleMultiplier = 1.0f + weightRatio * 0.5f;
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInventory.AddFishInstance(fishInstance);

            
            audioManager?.PlaySFX("Click");
            

            Destroy(gameObject);
        }
    }
}