using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fish Data")]
public class FishData : ScriptableObject
{
    public float CostPerKg;
    public float baseProgressThreshold;
    public float baseSlippage;
    public float baseProgressDrain;
    public float minTargetMoveInterval;
    public float maxTargetMoveInterval;
    public float playerPullAgainst;
    public string fishName;
    public float minWeight;
    public float maxWeight;
    public int rarity;

    public GameObject fishPrefab;
    public Sprite fishIcon;

    [Header("Dont Touch")]
    public int sellPrice;
}
