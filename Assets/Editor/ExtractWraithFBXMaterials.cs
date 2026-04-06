using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using System.IO;

public class ExtractWraithFBXMaterials
{
    [MenuItem("Tools/Extract Wraith FBX Materials")]
    public static void ExtractMaterials()
    {
        string assetPath = "Assets/Characters/Wraith_Meshy/Meshy_AI_Humanoid_full_body_vi_biped_Character_output.fbx";
        string outputFolder = "Assets/Characters/Wraith_Meshy";

        // Load the FBX model
        var fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (fbxModel == null)
        {
            Debug.LogError("Could not load FBX model at: " + assetPath);
            return;
        }

        // Get the model importer
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("Could not get ModelImporter for: " + assetPath);
            return;
        }

        // Extract materials by getting all materials from the FBX
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
                extractedCount++;
            }
        }

        // Apply the changes
        importer.SaveAndReimport();

        Debug.Log("Successfully extracted " + extractedCount + " materials from " + assetPath + " to " + outputFolder);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
