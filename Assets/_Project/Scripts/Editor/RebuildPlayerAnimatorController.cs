using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class RebuildPlayerAnimatorController
{
    [MenuItem("Tools/Rebuild Player Animator Controller")]
    public static void Create()
    {
        string path = "Assets/_Project/Animations/PlayerController.controller";

        // Delete existing so we start clean
        AssetDatabase.DeleteAsset(path);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Parameters
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveZ", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        // Remove the default empty state
        if (sm.states.Length > 0)
            sm.RemoveState(sm.states[0].state);

        // Load clips
        var aimClip = GetClipFromFBX("Assets/Characters/Player1_aim.fbx");
        var walkFrontClip = GetClipFromFBX("Assets/Characters/Player1_WalkFront.fbx");
        var walkBackClip = GetClipFromFBX("Assets/Characters/Player1_WalkBack.fbx");
        var strafeLeftClip = GetClipFromFBX("Assets/Characters/Player1_LeftStrafe.fbx");
        var strafeRightClip = GetClipFromFBX("Assets/Characters/Player1_RightStrafe.fbx");
        var deathClip = GetClipFromFBX("Assets/Characters/Player1_Death.fbx");
        var reviveClip = GetClipFromFBX("Assets/Characters/Player1_Revive.fbx");

        // ── Idle state (Aim) ──
        var idleState = sm.AddState("Idle", new Vector3(250, 0, 0));
        idleState.motion = aimClip;
        sm.defaultState = idleState;

        // ── Locomotion blend tree (2D Simple Directional) ──
        BlendTree blendTree;
        var locomotionState = controller.CreateBlendTreeInController("Locomotion", out blendTree, 0);

        // Position the state in the graph
        // Find and reposition it
        var states = sm.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state == locomotionState)
            {
                var cs = states[i];
                cs.position = new Vector3(250, 120, 0);
                states[i] = cs;
                break;
            }
        }
        sm.states = states;

        blendTree.blendType = BlendTreeType.SimpleDirectional2D;
        blendTree.blendParameter = "MoveX";
        blendTree.blendParameterY = "MoveZ";

        // Add motions: (clip, position in 2D space)
        // Forward = +Z, Back = -Z, Left = -X, Right = +X
        blendTree.AddChild(walkFrontClip, new Vector2(0f, 1f));
        blendTree.AddChild(walkBackClip, new Vector2(0f, -1f));
        blendTree.AddChild(strafeLeftClip, new Vector2(-1f, 0f));
        blendTree.AddChild(strafeRightClip, new Vector2(1f, 0f));

        // ── Death state ──
        var deathState = sm.AddState("Death", new Vector3(500, 0, 0));
        deathState.motion = deathClip;

        // ── Revive state ──
        var reviveState = sm.AddState("Revive", new Vector3(500, 120, 0));
        reviveState.motion = reviveClip;

        // ── Transitions ──

        // Idle -> Locomotion (IsMoving = true)
        var idleToLoco = idleState.AddTransition(locomotionState);
        idleToLoco.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        idleToLoco.hasExitTime = false;
        idleToLoco.duration = 0.15f;

        // Locomotion -> Idle (IsMoving = false)
        var locoToIdle = locomotionState.AddTransition(idleState);
        locoToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        locoToIdle.hasExitTime = false;
        locoToIdle.duration = 0.2f;

        // AnyState -> Death (Die trigger)
        var anyToDeath = sm.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Die");
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.1f;

        // Revive -> Idle (exit time)
        var reviveToIdle = reviveState.AddTransition(idleState);
        reviveToIdle.hasExitTime = true;
        reviveToIdle.exitTime = 0.9f;
        reviveToIdle.duration = 0.25f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RebuildPlayerAnimatorController] PlayerController rebuilt at {path} with 2D blend tree locomotion");
    }

    private static AnimationClip GetClipFromFBX(string fbxPath)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        Debug.LogWarning($"[RebuildPlayerAnimatorController] No clip found in {fbxPath}");
        return null;
    }
}
