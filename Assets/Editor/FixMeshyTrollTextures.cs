using UnityEditor;
using UnityEngine;

public class FixMeshyTrollTextures
{
    [MenuItem("Tools/Fix Meshy Troll Textures")]
    public static void FixTextures()
    {
        string[] texturePaths = new string[]
        {
            "Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0.png",
            "Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0_normal.png",
            "Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0_metallic.png",
            "Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0_roughness.png"
        };

        foreach (string path in texturePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"Could not find texture importer for {path}");
                continue;
            }

            // Set common texture settings
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = false;
            importer.ignorePngGamma = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.mipmapFilter = TextureImporterMipFilter.BoxFilter;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Repeat;
            importer.wrapModeW = TextureWrapMode.Repeat;

            // Set compression
            TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
            platformSettings.format = TextureImporterFormat.DXT5;
            platformSettings.maxTextureSize = 2048;
            platformSettings.compressionQuality = 100;
            importer.SetPlatformTextureSettings(platformSettings);

            // Special settings for specific texture types
            if (path.Contains("_normal"))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
                importer.normalmap = true;
            }
            else if (path.Contains("_metallic") || path.Contains("_roughness"))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
            }

            importer.SaveAndReimport();
            Debug.Log($"Fixed texture: {path}");
        }

        AssetDatabase.Refresh();
        Debug.Log("All Meshy Troll textures have been fixed!");
    }
}
