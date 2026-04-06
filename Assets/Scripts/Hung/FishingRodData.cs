using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rod", menuName = "Fishing/Rod Data")]
public class FishingRodData : ScriptableObject
{
    public string rodName;
    public float pullPower;

    [Header("Shop & Inventory")]
    public int price;
    public Sprite rodIcon;
}