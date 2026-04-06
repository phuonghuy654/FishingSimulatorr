using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RodDatabase", menuName = "Fishing/Rod Database")]
public class RodDatabaseSO : ScriptableObject
{
    public List<FishingRodData> allRods;
}