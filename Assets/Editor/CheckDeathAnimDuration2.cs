using UnityEditor;
using UnityEngine;

public class CheckDeathAnimDuration2
{
    public static void Execute()
    {
        // Load the Death animation clip directly
        var deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Characters/Player1_Death.fbx");
        if (deathClip != null)
        {
            Debug.Log($"[CheckDeathAnimDuration2] Player1_Death.fbx loaded as AnimationClip");
            Debug.Log($"  Duration: {deathClip.length}s");
            Debug.Log($"  Frame Rate: {deathClip.frameRate}");
            Debug.Log($"  Total Frames: {deathClip.frameRate * deathClip.length}");
        }
        else
        {
            Debug.LogWarning("Could not load Player1_Death.fbx as AnimationClip");
        }

        // Try to find animation clips in the project
        var guids = AssetDatabase.FindAssets("Player1_Death", new[] { "Assets/Characters" });
        Debug.Log($"[CheckDeathAnimDuration2] Found {guids.Length} assets matching 'Player1_Death'");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"  Path: {path}");
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            Debug.Log($"    Type: {obj.GetType().Name}");
        }
    }
}
