using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDatabase", menuName = "Fishing/Map Database")]
public class MapDatabaseSO : ScriptableObject
{
    public List<MapData> allMaps;
}