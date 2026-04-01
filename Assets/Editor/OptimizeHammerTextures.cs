using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;

public class OptimizeHammerTextures
{
    [MenuItem("Tools/Optimize Hammer Textures")]
    public static void Execute()
    {
        Debug.Log("=== OPTIMIZING HAMMER TEXTURES ===\n");

        // Texture paths
        string[] texturePaths = new string[]
        {
            "Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture.png",
            "Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_normal.png",
            "Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_metallic.png",
            "Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_roughness.png"
        };

        foreach (string path in texturePaths)
        {
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
            if (path.Contains("_normal"))
            {
                Debug.Log("  → Configuring as Normal Map");
                importer.textureType = TextureImporterType.NormalMap;
                importer.normalmap = true;
            }
            else if (path.Contains("_metallic") || path.Contains("_roughness"))
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

        Debug.Log("=== TEXTURE OPTIMIZATION COMPLETE ===");
        AssetDatabase.Refresh();
    }
}
