using UnityEditor;
using UnityEngine;

public class InspectAndRenameAnimations
{
    public static void Execute()
    {
        string path = "Assets/Characters/Player1.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null)
        {
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importer.defaultClipAnimations;
            }

            if (clips != null && clips.Length > 0)
            {
                bool changed = false;
                for (int i = 0; i < clips.Length; i++)
                {
                    Debug.Log("Found animation clip: " + clips[i].name);
                    if (clips[i].name.Contains("Take 001") || clips[i].name.Contains("mixamo.com"))
                    {
                        clips[i].name = "Idle";
                        changed = true;
                        Debug.Log("Renamed to Idle");
                    }
                }

                if (changed)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    Debug.Log("Successfully renamed animations and reimported.");
                }
                else
                {
                    Debug.Log("No animations needed renaming.");
                }
            }
            else
            {
                Debug.Log("No animation clips found in " + path);
            }
        }
        else
        {
            Debug.LogError("Could not find ModelImporter for " + path);
        }
    }
}