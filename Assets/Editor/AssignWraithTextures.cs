using UnityEditor;
using UnityEngine;
using System.IO;

public class AssignWraithTextures
{
    [MenuItem("Tools/Assign Wraith Textures")]
    public static void AssignTextures()
    {
        string materialsFolder = "Assets/Characters/Wraith_Meshy";
        string[] matFiles = Directory.GetFiles(materialsFolder, "*.mat");

        if (matFiles.Length == 0)
        {
            Debug.LogWarning("No .mat files found in " + materialsFolder);
            return;
        }

        // Load the textures
        Texture2D normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_texture_0_normal.png");
        Texture2D metallicTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_texture_0_metallic.png");

        if (normalTexture == null || metallicTexture == null)
        {
            Debug.LogError("Could not load one or more textures");
            return;
        }

        int configuredCount = 0;

        foreach (string matFile in matFiles)
        {
            string assetPath = matFile.Replace("\\", "/");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (material != null)
            {
                // Set normal map texture
                material.SetTexture("_NormalMap", normalTexture);

                // Set metallic map texture
                material.SetTexture("_MetallicGlossMap", metallicTexture);

                // Mark as modified
                EditorUtility.SetDirty(material);
                configuredCount++;

                Debug.Log("Assigned textures to material: " + assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Successfully assigned normal and metallic textures to " + configuredCount + " materials");
    }
}
