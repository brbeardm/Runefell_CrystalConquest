using UnityEditor;
using UnityEngine;

public class CheckDeathAnimDuration
{
    public static void Execute()
    {
        // Load the Player1_Death FBX
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Player1_Death.fbx");
        if (fbx == null)
        {
            Debug.LogError("Could not load Player1_Death.fbx");
            return;
        }

        // Get all animation clips from the FBX
        var clips = AnimationUtility.GetAnimationClips(fbx);
        Debug.Log($"[CheckDeathAnimDuration] Found {clips.Length} animation clips in Player1_Death.fbx");

        foreach (var clip in clips)
        {
            Debug.Log($"  Clip: {clip.name}, Duration: {clip.length}s, Frames: {clip.frameRate * clip.length}");
        }

        // Also check the animator controller
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/PlayerController.controller");
        if (controller != null)
        {
            Debug.Log("[CheckDeathAnimDuration] PlayerController loaded");
        }
    }
}
