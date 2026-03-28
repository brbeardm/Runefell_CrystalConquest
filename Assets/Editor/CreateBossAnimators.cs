using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Creates Animator Controller assets for each boss type from their FBX animation clips.
/// Menu: Tools > Create Boss Animator Controllers
///
/// Expected FBX naming in Assets/Characters/:
///   {BossName}_TPose.fbx, {BossName}_Walking.fbx, {BossName}_Attack.fbx, {BossName}_Dying.fbx
///   OR
///   {BossName}-TPose.fbx, {BossName}-Walking.fbx, {BossName}-Attack.fbx, {BossName}-Death.fbx
///
/// Creates controllers at: Assets/_Project/Animations/{BossName}Controller.controller
/// </summary>
public class CreateBossAnimators
{
    private static readonly string[] BossNames = { "Orc", "Troll" };

    [MenuItem("Tools/Create Boss Animator Controllers")]
    public static void Execute()
    {
        // Ensure output directory exists
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            AssetDatabase.CreateFolder("Assets/_Project", "Animations");
        }

        foreach (var bossName in BossNames)
        {
            CreateControllerForBoss(bossName);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Boss Animator Controllers created successfully.");
    }

    private static void CreateControllerForBoss(string bossName)
    {
        // Match existing controller names (BossOrcController, BossTrollController, etc.)
        string controllerPath = $"Assets/_Project/Animations/Boss{bossName}Controller.controller";

        // Find animation clips from FBX files
        var walkClip = FindClipInFBX(bossName, "Walking");
        var attackClip = FindClipInFBX(bossName, "Attack");
        var dieClip = FindClipInFBX(bossName, "Dying", "Death");
        var idleClip = FindClipInFBX(bossName, "TPose");

        if (walkClip == null)
        {
            Debug.LogWarning($"[{bossName}] Walking clip not found — skipping controller creation.");
            return;
        }

        // Create or overwrite controller
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add parameters
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Walk", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        // Get the base layer state machine
        var rootStateMachine = controller.layers[0].stateMachine;

        // ── States ───────────────────────────────────────────────────────

        // Idle (default state — uses TPose or first frame of walk)
        var idleState = rootStateMachine.AddState("Idle", new Vector3(250, 0, 0));
        if (idleClip != null)
            idleState.motion = idleClip;
        rootStateMachine.defaultState = idleState;

        // Walk (looping)
        var walkState = rootStateMachine.AddState("Walk", new Vector3(250, 100, 0));
        if (walkClip != null)
        {
            walkState.motion = walkClip;
            // Ensure walk clip loops
            SetClipLooping(walkClip, true);
        }

        // Attack (one-shot)
        var attackState = rootStateMachine.AddState("Attack", new Vector3(500, 50, 0));
        if (attackClip != null)
        {
            attackState.motion = attackClip;
            SetClipLooping(attackClip, false);
        }

        // Die (one-shot, no exit)
        var dieState = rootStateMachine.AddState("Die", new Vector3(250, 250, 0));
        if (dieClip != null)
        {
            dieState.motion = dieClip;
            SetClipLooping(dieClip, false);
        }

        // ── Transitions ──────────────────────────────────────────────────

        // Idle → Walk (when IsWalking becomes true)
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.15f;

        // Walk → Idle (when IsWalking becomes false)
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.15f;

        // Any State → Attack (trigger)
        var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.1f;

        // Attack → Walk (after clip finishes, resume walking)
        var attackToWalk = attackState.AddTransition(walkState);
        attackToWalk.hasExitTime = true;
        attackToWalk.exitTime = 0.95f;
        attackToWalk.duration = 0.15f;

        // Any State → Die (trigger, no exit)
        var anyToDie = rootStateMachine.AddAnyStateTransition(dieState);
        anyToDie.AddCondition(AnimatorConditionMode.If, 0, "Die");
        anyToDie.hasExitTime = false;
        anyToDie.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        Debug.Log($"[{bossName}] Animator Controller created at {controllerPath}" +
            $" — Walk:{(walkClip != null ? "OK" : "MISSING")}" +
            $" Attack:{(attackClip != null ? "OK" : "MISSING")}" +
            $" Die:{(dieClip != null ? "OK" : "MISSING")}" +
            $" Idle:{(idleClip != null ? "OK" : "MISSING")}");
    }

    /// <summary>
    /// Finds an animation clip inside an FBX file. Tries multiple naming conventions.
    /// </summary>
    private static AnimationClip FindClipInFBX(string bossName, params string[] clipSuffixes)
    {
        foreach (var suffix in clipSuffixes)
        {
            // Try underscore and hyphen naming
            string[] patterns = {
                $"Assets/Characters/{bossName}_{suffix}.fbx",
                $"Assets/Characters/{bossName}-{suffix}.fbx",
                $"Assets/Characters/{bossName}_{suffix.ToLower()}.fbx",
                $"Assets/Characters/{bossName}-{suffix.ToLower()}.fbx",
                $"Assets/Characters/{bossName}_New_{suffix}.fbx",
                $"Assets/Characters/{bossName}_New_{suffix.ToLower()}.fbx",
            };

            foreach (var path in patterns)
            {
                var objects = AssetDatabase.LoadAllAssetsAtPath(path);
                if (objects == null) continue;

                foreach (var obj in objects)
                {
                    if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                        return clip;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the loop flag on an animation clip's import settings.
    /// </summary>
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
