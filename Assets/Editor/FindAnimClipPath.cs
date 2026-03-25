using UnityEngine;
using UnityEditor;

public class FindAnimClipPath
{
    [MenuItem("Tools/Find Anim Clip Path")]
    public static void Execute()
    {
        // Load all assets at the FBX path and find AnimationClips
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/Player1.fbx");
        foreach (var a in assets)
        {
            if (a is AnimationClip clip)
                Debug.Log($"Clip: '{clip.name}' | path: Assets/Characters/Player1.fbx | isHumanMotion={clip.isHumanMotion}");
        }
    }
}
