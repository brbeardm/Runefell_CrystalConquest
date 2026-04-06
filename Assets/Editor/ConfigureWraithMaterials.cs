using UnityEditor;
using UnityEngine;
using System.IO;

public class ConfigureWraithMaterials
{
    [MenuItem("Tools/Configure Wraith Materials")]
    public static void ConfigureMaterials()
    {
        string materialsFolder = "Assets/Characters/Wraith_Meshy";
        string[] matFiles = Directory.GetFiles(materialsFolder, "*.mat");

        if (matFiles.Length == 0)
        {
            Debug.LogWarning("No .mat files found in " + materialsFolder);
            return;
        }

        // Load the URP Lit shader
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("Could not find Universal Render Pipeline/Lit shader");
            return;
        }

        // Load textures
        Texture2D albedoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_texture_0.png");
        Texture2D normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_texture_0_normal.png");
        Texture2D metallicTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_texture_0_metallic.png");

        if (albedoTexture == null || normalTexture == null || metallicTexture == null)
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
                // Set shader
                material.shader = urpLitShader;

                // Set textures
                material.SetTexture("_BaseMap", albedoTexture);
                material.SetTexture("_NormalMap", normalTexture);
                material.SetTexture("_MetallicGlossMap", metallicTexture);

                // Mark as modified
                EditorUtility.SetDirty(material);
                configuredCount++;

                Debug.Log("Configured material: " + assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Successfully configured " + configuredCount + " materials");
    }
}
