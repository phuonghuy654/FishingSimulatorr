using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fishing Spot", menuName = "Fishing/Fishing Spot Data")]
[System.Serializable]
public class FishingSpotData : ScriptableObject
{
    public FishSpawnInfo[] availableFish;
}

[System.Serializable]
public class FishSpawnInfo
{
    public FishData fishData;
    public float spawnChance;
}

