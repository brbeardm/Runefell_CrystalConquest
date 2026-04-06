using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupWraithMaterials
{
    [MenuItem("Tools/Setup Wraith Materials")]
    public static void SetupMaterials()
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

        // Load the base texture
        Texture2D baseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_texture_0.png");

        if (baseTexture == null)
        {
            Debug.LogError("Could not load base texture");
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

                // Set base map texture
                material.SetTexture("_BaseMap", baseTexture);

                // Mark as modified
                EditorUtility.SetDirty(material);
                configuredCount++;

                Debug.Log("Configured material: " + assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Successfully configured " + configuredCount + " materials with URP Lit shader and base texture");
    }
}
