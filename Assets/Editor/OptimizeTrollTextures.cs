using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

public class OptimizeTrollTextures
{
    [MenuItem("Tools/Optimize Troll Textures")]
    public static void Execute()
    {
        Debug.Log("=== OPTIMIZING TROLL TEXTURES ===\n");

        // Find all textures in the Troll_Idle_5meters.fbm folder
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Characters/Troll_Idle_5meters.fbm" });
        
        if (textureGuids.Length == 0)
        {
            Debug.LogWarning("No textures found in Troll_Idle_5meters.fbm folder!");
            return;
        }

        Debug.Log($"Found {textureGuids.Length} textures to optimize\n");

        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (importer == null)
            {
                Debug.LogWarning($"Could not find texture importer for: {path}");
                continue;
            }

            string textureName = System.IO.Path.GetFileNameWithoutExtension(path);
            Debug.Log($"Processing: {textureName}");

            // Common settings for all textures
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;

            // Specific settings based on texture type
            if (path.Contains("_normal") || path.Contains("_Normal"))
            {
                Debug.Log("  → Configuring as Normal Map");
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
            }
            else if (path.Contains("_metallic") || path.Contains("_roughness") || path.Contains("_Metallic") || path.Contains("_Roughness"))
            {
                Debug.Log("  → Configuring as Mask/Detail");
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false; // Linear color space for masks
            }
            else
            {
                Debug.Log("  → Configuring as Diffuse/Albedo");
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true; // sRGB for color textures
            }

            // Set max texture size for quality
            importer.maxTextureSize = 2048;

            // Apply changes
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            Debug.Log($"  ✓ Optimized\n");
        }

        Debug.Log("=== TROLL TEXTURE OPTIMIZATION COMPLETE ===");
        AssetDatabase.Refresh();
    }
}
