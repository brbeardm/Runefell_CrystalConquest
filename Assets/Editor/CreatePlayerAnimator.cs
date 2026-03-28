using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Creates a Player Animator Controller with Aim (idle) and Death states.
/// Menu: Tools > Create Player Animator Controller
/// </summary>
public class CreatePlayerAnimator
{
    [MenuItem("Tools/Create Player Animator Controller")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            AssetDatabase.CreateFolder("Assets/_Project", "Animations");
        }

        string controllerPath = "Assets/_Project/Animations/PlayerController.controller";

        var aimClip = FindClip("Assets/Characters/Player1_aim.fbx");
        var deathClip = FindClip("Assets/Characters/Player1_Death.fbx");

        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var rootSM = controller.layers[0].stateMachine;

        // Aim state (default, looping)
        var aimState = rootSM.AddState("Aim", new Vector3(250, 0, 0));
        if (aimClip != null)
        {
            aimState.motion = aimClip;
            SetClipLooping(aimClip, true);
        }
        rootSM.defaultState = aimState;

        // Death state (one-shot, no exit)
        var deathState = rootSM.AddState("Death", new Vector3(250, 150, 0));
        if (deathClip != null)
        {
            deathState.motion = deathClip;
            SetClipLooping(deathClip, false);
        }

        // Any State → Death (trigger)
        var anyToDeath = rootSM.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Die");
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"PlayerController created at {controllerPath}" +
            $" — Aim:{(aimClip != null ? "OK" : "MISSING")}" +
            $" Death:{(deathClip != null ? "OK" : "MISSING")}");
    }

    private static AnimationClip FindClip(string path)
    {
        var objects = AssetDatabase.LoadAllAssetsAtPath(path);
        if (objects == null) return null;

        foreach (var obj in objects)
        {
            if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        return null;
    }

    private static void SetClipLooping(AnimationClip clip, bool loop)
    {
        string path = AssetDatabase.GetAssetPath(clip);
        if (string.IsNullOrEmpty(path)) return;

        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) return;

        var clips = importer.clipAnimations;
        if (clips.Length == 0)
            clips = importer.defaultClipAnimations;

        bool changed = false;
        foreach (var c in clips)
        {
            if (c.loopTime != loop)
            {
                c.loopTime = loop;
                changed = true;
            }
        }

        if (changed)
        {
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }
    }
}
