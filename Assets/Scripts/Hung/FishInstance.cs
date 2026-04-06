using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FishInstance
{
    public FishData baseData;
    public float actualWeight;

    public FishInstance(FishData data)
    {
        baseData = data;
        actualWeight = Random.Range(data.minWeight, data.maxWeight);

    }
}
