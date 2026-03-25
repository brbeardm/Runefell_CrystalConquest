using UnityEngine;
using UnityEditor;

public class CheckAndFixAnimation
{
    [MenuItem("Tools/Check And Fix Animation")]
    public static void Execute()
    {
        string fbxPath = "Assets/Characters/Player1.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null) { Debug.LogError("No importer found"); return; }

        // Log current state
        var clips = importer.clipAnimations;
        var defaults = importer.defaultClipAnimations;
        Debug.Log($"Custom clips: {clips?.Length ?? 0}, Default clips: {defaults?.Length ?? 0}");

        ModelImporterClipAnimation[] toUse = (clips != null && clips.Length > 0) ? clips : defaults;
        foreach (var c in toUse)
        {
            Debug.Log($"  Clip '{c.name}': lockRootHeightY={c.lockRootHeightY}, lockRootPositionXZ={c.lockRootPositionXZ}, loopTime={c.loopTime}, loopPose={c.loopPose}");
        }

        // Force override with explicit settings
        ModelImporterClipAnimation[] newClips = new ModelImporterClipAnimation[toUse.Length];
        for (int i = 0; i < toUse.Length; i++)
        {
            newClips[i] = toUse[i];
            newClips[i].lockRootRotation = true;
            newClips[i].lockRootHeightY = true;
            newClips[i].lockRootPositionXZ = true;
            newClips[i].loopTime = true;
            newClips[i].loopPose = true;
            // Also set the loop match settings
            newClips[i].cycleOffset = 0f;
            Debug.Log($"  -> Set clip '{newClips[i].name}' bake-in-place");
        }

        importer.clipAnimations = newClips;
        importer.SaveAndReimport();
        Debug.Log("Done reimporting Player1.fbx");
    }
}
