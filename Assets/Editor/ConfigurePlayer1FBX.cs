using UnityEditor;
using UnityEngine;

public class ConfigurePlayer1FBX
{
    public static void Execute()
    {
        string path = "Assets/Characters/Player1.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.SaveAndReimport();
            Debug.Log("Successfully configured Player1.fbx to Humanoid.");
        }
        else
        {
            Debug.LogError("Could not find ModelImporter for " + path);
        }
    }
}