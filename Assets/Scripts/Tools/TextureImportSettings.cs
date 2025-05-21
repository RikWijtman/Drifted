using UnityEditor;
using UnityEngine;

public class TextureImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;

        // Stel Filter Mode in op Point
        textureImporter.filterMode = FilterMode.Point;

        // Optioneel: Zet de Compression uit
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

        // Optioneel: Stel MipMaps uit
        textureImporter.mipmapEnabled = false;
    }
}
