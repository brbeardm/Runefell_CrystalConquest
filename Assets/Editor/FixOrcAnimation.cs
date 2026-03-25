using UnityEngine;
using UnityEditor;

public class FixOrcAnimation
{
    [MenuItem("Tools/Fix Orc Animation")]
    public static void Execute()
    {
        string fbxPath = "Assets/Characters/enemy_orc.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null) { Debug.LogError("No importer at " + fbxPath); return; }

        // Log current clips
        var clips = importer.clipAnimations;
        var defaults = importer.defaultClipAnimations;
        var toUse = (clips != null && clips.Length > 0) ? clips : defaults;

        Debug.Log($"Orc clips: {toUse.Length}");
        foreach (var c in toUse)
            Debug.Log($"  '{c.name}' lockRootHeightY={c.lockRootHeightY} lockRootPositionXZ={c.lockRootPositionXZ} loopTime={c.loopTime}");

        // Bake ALL root motion into pose — no XZ drift, no Y bob
        ModelImporterClipAnimation[] newClips = new ModelImporterClipAnimation[toUse.Length];
        for (int i = 0; i < toUse.Length; i++)
        {
            newClips[i] = toUse[i];
            newClips[i].lockRootRotation    = true;
            newClips[i].lockRootHeightY     = true;
            newClips[i].lockRootPositionXZ  = true;
            newClips[i].loopTime            = true;
            newClips[i].loopPose            = true;
        }

        importer.clipAnimations = newClips;
        importer.SaveAndReimport();
        Debug.Log("Orc FBX reimported — root motion baked into pose. XZ drift eliminated.");
    }
}
