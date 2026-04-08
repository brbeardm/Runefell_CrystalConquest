using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class CreateSorcererAnimatorController
{
    [MenuItem("Tools/Create Sorcerer Animator Controller")]
    public static void Create()
    {
        string path = "Assets/_Project/Animations/BossSorcererController.controller";

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Add parameters (same as other animated bosses)
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Walk", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Taunt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        // Remove the default empty state
        if (sm.states.Length > 0)
            sm.RemoveState(sm.states[0].state);

        // Load animation clips from FBX files
        var walkClip = GetClipFromFBX("Assets/Characters/Sorcerer_Meshy/Sorcerer_Walking.fbx");
        var attackClip = GetClipFromFBX("Assets/Characters/Sorcerer_Meshy/Sorcerer_Attack.fbx");
        var tauntClip = GetClipFromFBX("Assets/Characters/Sorcerer_Meshy/Sorcerer_Taunt.fbx");
        var deathClip = GetClipFromFBX("Assets/Characters/Sorcerer_Meshy/Sorcerer_Death.fbx");

        // Create states
        var idleState = sm.AddState("Idle", new Vector3(250, 0, 0));
        idleState.motion = walkClip; // Use walk clip for idle (bosses walk by default)

        var walkState = sm.AddState("Walk", new Vector3(250, 100, 0));
        walkState.motion = walkClip;

        var attackState = sm.AddState("Attack", new Vector3(500, 0, 0));
        attackState.motion = attackClip;

        var tauntState = sm.AddState("Taunt", new Vector3(500, 100, 0));
        tauntState.motion = tauntClip;

        var dieState = sm.AddState("Die", new Vector3(250, 250, 0));
        dieState.motion = deathClip;

        sm.defaultState = idleState;

        // Idle -> Walk (IsWalking = true)
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.15f;

        // Walk -> Idle (IsWalking = false)
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.15f;

        // AnyState -> Attack (trigger)
        var anyToAttack = sm.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.1f;

        // AnyState -> Taunt (trigger)
        var anyToTaunt = sm.AddAnyStateTransition(tauntState);
        anyToTaunt.AddCondition(AnimatorConditionMode.If, 0, "Taunt");
        anyToTaunt.hasExitTime = false;
        anyToTaunt.duration = 0.1f;

        // AnyState -> Die (trigger)
        var anyToDie = sm.AddAnyStateTransition(dieState);
        anyToDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
        anyToDie.hasExitTime = false;
        anyToDie.duration = 0.1f;

        // Attack -> Walk (exit time)
        var attackToWalk = attackState.AddTransition(walkState);
        attackToWalk.hasExitTime = true;
        attackToWalk.exitTime = 0.95f;
        attackToWalk.duration = 0.15f;

        // Taunt -> Walk (exit time)
        var tauntToWalk = tauntState.AddTransition(walkState);
        tauntToWalk.hasExitTime = true;
        tauntToWalk.exitTime = 0.95f;
        tauntToWalk.duration = 0.15f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CreateSorcererAnimatorController] BossSorcererController created at " + path);
    }

    private static AnimationClip GetClipFromFBX(string fbxPath)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        Debug.LogWarning($"[CreateSorcererAnimatorController] No clip found in {fbxPath}");
        return null;
    }
}
