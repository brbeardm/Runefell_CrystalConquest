using UnityEngine;
using UnityEditor;

public class BakePlayerAnimationInPlace
{
    [MenuItem("Tools/Bake Player Animation In Place")]
    public static void Execute()
    {
        // Find the Player1 FBX importer
        string fbxPath = "Assets/Characters/Player1.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"Could not find ModelImporter at {fbxPath}");
            return;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            // Fall back to default clips
            clips = importer.defaultClipAnimations;
            Debug.Log($"Using {clips.Length} default clip(s)");
        }
        else
        {
            Debug.Log($"Found {clips.Length} custom clip(s)");
        }

        bool changed = false;
        foreach (var clip in clips)
        {
            Debug.Log($"Clip: {clip.name} | lockRootRotation={clip.lockRootRotation} | lockRootHeightY={clip.lockRootHeightY} | lockRootPositionXZ={clip.lockRootPositionXZ} | loop={clip.loopTime}");

            // Bake root motion into pose — lock all root motion axes
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;       // bake Y into pose (stops vertical bobbing)
            clip.lockRootPositionXZ = true;    // bake XZ into pose (stops forward drift)
            clip.loopTime = true;              // ensure it loops
            clip.loopPose = true;              // smooth loop
            changed = true;
        }

        if (changed)
        {
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            Debug.Log("Player1.fbx reimported with root motion baked into pose.");
        }
    }
}
