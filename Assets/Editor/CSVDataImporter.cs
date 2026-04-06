using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public class CSVDataImporter : EditorWindow
{
    
    public TextAsset fishDataCSV;
    public TextAsset rodDataCSV;
    public TextAsset spotDataCSV;

    
    
    private string fishSavePath = "Assets/DataExport/Fish/SO";
    private string rodSavePath = "Assets/DataExport/Rods/SO";
    private string spotSavePath = "Assets/DataExport/Spots/SO";

    private string fishIconPath = "Assets/DataExport/Fish/Icons";
    private string fishPrefabPath = "Assets/DataExport/Fish/Prefabs";
    private string rodIconPath = "Assets/DataExport/Rods/Icons";
    

    [MenuItem("Tools/CSV Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<CSVDataImporter>("CSV Data Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV Importer", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Các đường dẫn sẽ trỏ vào 'Assets/DataExport/'. Hãy đảm bảo cấu trúc thư mục của bạn khớp.", MessageType.Info);

        
        EditorGUILayout.Space(10);
        GUILayout.Label("1. Kéo File CSV (TextAssets)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Kéo file CSV từ thư mục 'Assets/DataExport/CSV_Files/' của bạn vào đây.", MessageType.None);
        fishDataCSV = (TextAsset)EditorGUILayout.ObjectField("Fish Data (CSV)", fishDataCSV, typeof(TextAsset), false);
        rodDataCSV = (TextAsset)EditorGUILayout.ObjectField("Rod Data (CSV)", rodDataCSV, typeof(TextAsset), false);
        spotDataCSV = (TextAsset)EditorGUILayout.ObjectField("Spot Data (CSV)", spotDataCSV, typeof(TextAsset), false);

        
        EditorGUILayout.Space(10);
        GUILayout.Label("2. Đường Dẫn (Đang Sử Dụng)", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Fish SOs Path:", fishSavePath);
        EditorGUILayout.LabelField("Rod SOs Path:", rodSavePath);
        EditorGUILayout.LabelField("Spot SOs Path:", spotSavePath);
        EditorGUILayout.LabelField("Fish Icons Path:", fishIconPath);
        EditorGUILayout.LabelField("Fish Prefabs Path:", fishPrefabPath);
        EditorGUILayout.LabelField("Rod Icons Path:", rodIconPath);


        
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Import ALL Fish Data"))
        {
            if (fishDataCSV != null)
                ImportFishData();
            else
                Debug.LogError("Chưa kéo file Fish Data CSV vào!");
        }

        if (GUILayout.Button("Import ALL Rod Data"))
        {
            if (rodDataCSV != null)
                ImportRodData();
            else
                Debug.LogError("Chưa kéo file Rod Data CSV vào!");
        }

        if (GUILayout.Button("Import ALL Spot Data"))
        {
            if (spotDataCSV != null)
                ImportSpotData();
            else
                Debug.LogError("Chưa kéo file Spot Data CSV vào!");
        }
    }

    
    private void ImportFishData()
    {
        CheckAndCreateDirectory(fishSavePath);
        CheckAndCreateDirectory(fishIconPath);
        CheckAndCreateDirectory(fishPrefabPath);

        string[] lines = fishDataCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');

            
            
            

            if (fields.Length < 14)
            {
                Debug.LogWarning($"Skipping line {i}: Not enough fields. Expected 14, got {fields.Length}.");
                continue;
            }

            string fileName = fields[0];
            string assetPath = $"{fishSavePath}/{fileName}.asset";

            FishData fish = AssetDatabase.LoadAssetAtPath<FishData>(assetPath);
            if (fish == null)
            {
                fish = ScriptableObject.CreateInstance<FishData>();
                AssetDatabase.CreateAsset(fish, assetPath);
            }

            try
            {
                fish.fishName = fields[1];
                fish.CostPerKg = float.Parse(fields[2], CultureInfo.InvariantCulture);
                fish.baseProgressThreshold = float.Parse(fields[3], CultureInfo.InvariantCulture);
                fish.baseProgressDrain = float.Parse(fields[4], CultureInfo.InvariantCulture);
                fish.baseSlippage = float.Parse(fields[5], CultureInfo.InvariantCulture);
                fish.playerPullAgainst = float.Parse(fields[6], CultureInfo.InvariantCulture);
                fish.rarity = int.Parse(fields[7], CultureInfo.InvariantCulture);
                fish.minTargetMoveInterval = float.Parse(fields[8], CultureInfo.InvariantCulture);
                fish.maxTargetMoveInterval = float.Parse(fields[9], CultureInfo.InvariantCulture);
                fish.minWeight = float.Parse(fields[10], CultureInfo.InvariantCulture);
                fish.maxWeight = float.Parse(fields[11], CultureInfo.InvariantCulture);

                string iconAssetPath = $"{fishIconPath}/{fields[12]}";
                fish.fishIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconAssetPath);
                if (fish.fishIcon == null && !string.IsNullOrEmpty(fields[12])) Debug.LogWarning($"Không tìm thấy Icon: {iconAssetPath}");

                string prefabAssetPath = $"{fishPrefabPath}/{fields[13]}";
                fish.fishPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                if (fish.fishPrefab == null && !string.IsNullOrEmpty(fields[13])) Debug.LogWarning($"Không tìm thấy Prefab: {prefabAssetPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line {i} (Fish: {fileName}): {e.Message}");
                continue;
            }

            EditorUtility.SetDirty(fish);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("--- Import Dữ Liệu CÁ thành công! ---");
    }

    
    private void ImportRodData()
    {
        CheckAndCreateDirectory(rodSavePath);
        CheckAndCreateDirectory(rodIconPath);

        string[] lines = rodDataCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');

            

            if (fields.Length < 5)
            {
                Debug.LogWarning($"Skipping line {i}: Not enough fields. Expected 5, got {fields.Length}.");
                continue;
            }

            string fileName = fields[0];
            string assetPath = $"{rodSavePath}/{fileName}.asset";

            FishingRodData rod = AssetDatabase.LoadAssetAtPath<FishingRodData>(assetPath);
            if (rod == null)
            {
                rod = ScriptableObject.CreateInstance<FishingRodData>();
                AssetDatabase.CreateAsset(rod, assetPath);
            }

            try
            {
                rod.rodName = fields[1];
                rod.pullPower = float.Parse(fields[2], CultureInfo.InvariantCulture);
                rod.price = int.Parse(fields[3], CultureInfo.InvariantCulture);

                string iconAssetPath = $"{rodIconPath}/{fields[4]}";
                rod.rodIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconAssetPath);
                if (rod.rodIcon == null && !string.IsNullOrEmpty(fields[4])) Debug.LogWarning($"Không tìm thấy Icon: {iconAssetPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line {i} (Rod: {fileName}): {e.Message}");
                continue;
            }

            EditorUtility.SetDirty(rod);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("--- Import Dữ Liệu CẦN CÂU thành công! ---");
    }

    
    private void ImportSpotData()
    {
        CheckAndCreateDirectory(spotSavePath);

        string[] lines = spotDataCSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] fields = line.Split(',');

            

            if (fields.Length < 11)
            {
                Debug.LogWarning($"Skipping line {i}: Not enough fields. Expected 11, got {fields.Length}.");
                continue;
            }

            string fileName = fields[0];
            string assetPath = $"{spotSavePath}/{fileName}.asset";

            FishingSpotData spot = AssetDatabase.LoadAssetAtPath<FishingSpotData>(assetPath);
            if (spot == null)
            {
                spot = ScriptableObject.CreateInstance<FishingSpotData>();
                AssetDatabase.CreateAsset(spot, assetPath);
            }

            List<FishSpawnInfo> fishList = new List<FishSpawnInfo>();
            try
            {
                for (int j = 0; j < 5; j++)
                {
                    string fishFileName = fields[j * 2 + 1];
                    string fishChanceStr = fields[j * 2 + 2];

                    if (string.IsNullOrEmpty(fishFileName)) continue;

                    
                    
                    string fishAssetPath = $"{fishSavePath}/{fishFileName}.asset";
                    FishData fishData = AssetDatabase.LoadAssetAtPath<FishData>(fishAssetPath);

                    if (fishData != null)
                    {
                        FishSpawnInfo info = new FishSpawnInfo
                        {
                            fishData = fishData,
                            spawnChance = float.Parse(fishChanceStr, CultureInfo.InvariantCulture)
                        };
                        fishList.Add(info);
                    }
                    else
                    {
                        Debug.LogWarning($"Không tìm thấy FishData '{fishFileName}' tại '{fishAssetPath}' khi import '{fileName}' (Line {i})");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line {i} (Spot: {fileName}): {e.Message}");
                continue;
            }

            spot.availableFish = fishList.ToArray();
            EditorUtility.SetDirty(spot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("--- Import Dữ Liệu KHU VỰC CÂU thành công! ---");
    }

    
    private void CheckAndCreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log($"Đã tạo thư mục: {path}");
        }
    }
}