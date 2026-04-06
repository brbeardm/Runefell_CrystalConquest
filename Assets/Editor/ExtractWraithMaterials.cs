using UnityEditor;
using UnityEngine;
using System.IO;

public class ExtractWraithMaterials
{
    [MenuItem("Tools/Extract Wraith Materials")]
    public static void ExtractMaterials()
    {
        string assetPath = "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_Character_output.fbx";
        string outputFolder = "Assets/Characters/Wraith_Meshy";

        // Load the FBX asset
        var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (fbxAsset == null)
        {
            Debug.LogError("Could not load FBX asset at: " + assetPath);
            return;
        }

        // Get all materials from the FBX
        var materials = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        int extractedCount = 0;

        foreach (var asset in materials)
        {
            if (asset is Material material)
            {
                string materialName = material.name;
                string materialPath = Path.Combine(outputFolder, materialName + ".mat");

                // Create a new material asset
                Material newMaterial = new Material(material);
                AssetDatabase.CreateAsset(newMaterial, materialPath);
                Debug.Log("Extracted material: " + materialPath);
                extractedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (extractedCount > 0)
        {
            Debug.Log("Successfully extracted " + extractedCount + " materials to " + outputFolder);
        }
        else
        {
            Debug.LogWarning("No materials found to extract from the FBX file.");
        }
    }
}
