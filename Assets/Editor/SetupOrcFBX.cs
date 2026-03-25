using UnityEditor;
using UnityEngine;

public class SetupOrcFBX
{
    public static void Execute()
    {
        string fbxPath = "Assets/Characters/enemy_orc.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        
        if (importer != null)
        {
            // Set Rig to Humanoid
            importer.animationType = ModelImporterAnimationType.Human;
            
            // Set Animation to Loop Time
            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            if (clips != null && clips.Length > 0)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].loopTime = true;
                }
                importer.clipAnimations = clips;
            }
            
            importer.SaveAndReimport();
            Debug.Log("Configured enemy_orc.fbx import settings.");
        }
        else
        {
            Debug.LogError("Could not find enemy_orc.fbx at " + fbxPath);
        }
    }
}