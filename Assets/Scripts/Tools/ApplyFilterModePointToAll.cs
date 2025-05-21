using UnityEditor;
using UnityEngine;

public class ApplyFilterModePointToAll : Editor
{
    [MenuItem("Tools/Apply Point Filter Mode to All Textures")]
    public static void ApplyToAllTextures()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture");

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            if (textureImporter != null)
            {
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.mipmapEnabled = false;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        Debug.Log("Filter Mode set to Point for all textures.");
    }
}
