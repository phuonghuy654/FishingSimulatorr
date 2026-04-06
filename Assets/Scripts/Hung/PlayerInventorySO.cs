using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Fishing/Player Inventory")]
public class PlayerInventorySO : ScriptableObject
{
    [Header("Default Settings (New Game)")]
    [Tooltip("Drag the basic FishingRodData asset here")]
    public FishingRodData defaultRod;

    [Tooltip("Starting money for a new game")]
    public int startingMoney = 50;

    [Tooltip("Drag the default MapData asset (first map) here")]
    public MapData defaultMap;


    [Header("Current State")]
    public int money;
    public FishingRodData currentRod;
    public List<FishInstance> caughtFishes = new List<FishInstance>();

    [Tooltip("List of fishing rods the player owns")]
    public List<FishingRodData> ownedRods = new List<FishingRodData>();

    [Header("Map & Scene Data")]
    [Tooltip("The scene name of the player's current location")]
    public string currentSceneName;

    [Tooltip("List of scene names for maps the player has unlocked")]
    public List<string> unlockedSceneNames = new List<string>();


    [HideInInspector]
    public FishInstance temporaryFish;

    public void CreateTemporaryFish(FishData fishSpecies)
    {
        temporaryFish = new FishInstance(fishSpecies);
        Debug.Log("A " + fishSpecies.fishName + " is biting! Weight: " + temporaryFish.actualWeight.ToString("F2") + "kg");
    }

    public void AddTemporaryFishToInventory()
    {
        if (temporaryFish != null)
        {
            caughtFishes.Add(temporaryFish);
            Debug.Log("Successfully caught " + temporaryFish.baseData.fishName);
            temporaryFish = null;
        }
    }

    public void AddFishInstance(FishInstance fishToAdd)
    {
        if (fishToAdd != null)
        {
            caughtFishes.Add(fishToAdd);
            Debug.Log("Picked up " + fishToAdd.baseData.fishName + " and added to inventory.");
        }
        else
        {
            Debug.LogWarning("FishPickup tried to add a null fish!");
        }
    }

    public void SellAllFish()
    {
        if (caughtFishes.Count == 0)
        {
            Debug.Log("No fish to sell.");
            return;
        }

        int totalValue = 0;
        foreach (FishInstance fish in caughtFishes)
        {
            int price = Mathf.RoundToInt(fish.baseData.CostPerKg * fish.actualWeight);
            totalValue += price;
        }

        money += totalValue;
        caughtFishes.Clear();

        Debug.Log("Sold all fish for " + totalValue + " gold! Current money: " + money);
    }

    public List<FishInstance> GetCaughtFishes()
    {
        return caughtFishes;
    }

    public void ResetData()
    {
        money = startingMoney;
        currentRod = defaultRod;
        caughtFishes.Clear();
        temporaryFish = null;

        ownedRods.Clear();
        if (defaultRod != null && !ownedRods.Contains(defaultRod))
        {
            ownedRods.Add(defaultRod);
        }

        unlockedSceneNames.Clear();
        if (defaultMap != null)
        {
            unlockedSceneNames.Add(defaultMap.sceneName);
            currentSceneName = defaultMap.sceneName;
        }
        else
        {
            Debug.LogError("PlayerInventorySO is missing its Default Map!");
        }
    }
}