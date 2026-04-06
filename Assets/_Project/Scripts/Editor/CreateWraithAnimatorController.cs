using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public static class CreateWraithAnimatorController
{
    [MenuItem("Tools/Create Wraith Animator Controller")]
    public static void Create()
    {
        string path = "Assets/_Project/Animations/BossWraithController.controller";

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Add parameters
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Walk", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Taunt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        // Get the base layer state machine
        var sm = controller.layers[0].stateMachine;

        // Load animation clips from FBX files
        var walkClip = GetClipFromFBX("Assets/Characters/Wraith_Meshy/Wraith_Walking.fbx", "Wraith_Walking");
        var attackClip = GetClipFromFBX("Assets/Characters/Wraith_Meshy/Wraith_Attack.fbx", "Wraith_Attack");
        var tauntClip = GetClipFromFBX("Assets/Characters/Wraith_Meshy/Wraith_Taunt.fbx", "Wraith_Taunt");
        var deathClip = GetClipFromFBX("Assets/Characters/Wraith_Meshy/Wraith_Death.fbx", "Wraith_Death");

        // Remove the default empty state that Unity creates
        if (sm.states.Length > 0)
            sm.RemoveState(sm.states[0].state);

        // Create states
        var idleState = sm.AddState("Idle", new Vector3(250, 0, 0));
        idleState.motion = walkClip;

        var walkState = sm.AddState("Walk", new Vector3(250, 100, 0));
        walkState.motion = walkClip;

        var attackState = sm.AddState("Attack", new Vector3(500, 0, 0));
        attackState.motion = attackClip;

        var tauntState = sm.AddState("Taunt", new Vector3(500, 100, 0));
        tauntState.motion = tauntClip;

        var dieState = sm.AddState("Die", new Vector3(250, 250, 0));
        dieState.motion = deathClip;

        // Set Idle as default
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

        Debug.Log("[CreateWraithAnimatorController] BossWraithController created at " + path);
    }

    private static AnimationClip GetClipFromFBX(string fbxPath, string clipName)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var asset in assets)
        {
            if (asset is AnimationClip clip && clip.name == clipName)
                return clip;
        }

        // Fallback: return any animation clip found
        foreach (var asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }

        Debug.LogWarning($"[CreateWraithAnimatorController] No clip '{clipName}' found in {fbxPath}");
        return null;
    }
}
