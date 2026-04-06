using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Map", menuName = "Fishing/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Display Information")]
    public string mapName;
    [TextArea(3, 5)]
    public string mapDescription;
    public Sprite mapImage;

    [Header("System Information")]
    [Tooltip("The price to permanently unlock this map")]
    public int unlockPrice;

    [Tooltip("The EXACT name of the Scene file to load, e.g., '(1st)GameScene'")]
    public string sceneName;
}