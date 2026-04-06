using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetThumbnailExporter : EditorWindow
{
    [MenuItem("Assets/Export Thumbnail")]
    private static void ExportAssetThumbnail()
    {
        Object selectedObject = Selection.activeObject;

        if (selectedObject == null)
        {
            return;
        }

        Texture2D thumbnail = AssetPreview.GetAssetPreview(selectedObject);

        if (thumbnail == null)
        {
            return;
        }

        byte[] bytes = thumbnail.EncodeToPNG();
        if (bytes == null)
        {
            return;
        }

        string path = EditorUtility.SaveFilePanel(
            "Save Thumbnail PNG",
            "",
            selectedObject.name + "_thumbnail.png",
            "png");

        if (path.Length != 0)
        {
            File.WriteAllBytes(path, bytes);
        }
    }
}