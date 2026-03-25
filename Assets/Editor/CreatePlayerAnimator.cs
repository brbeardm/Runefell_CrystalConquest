using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class CreatePlayerAnimator
{
    public static void Execute()
    {
        string path = "Assets/_Project/Prefabs/Shooters/PlayerAnimator.controller";
        
        // Create Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        
        // Add Parameter
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        
        // Get Root State Machine
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // Create States
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        AnimatorState runState = rootStateMachine.AddState("Run");
        
        // Set Default State
        rootStateMachine.defaultState = idleState;
        
        // Find Idle Animation from Player1.fbx
        string fbxPath = "Assets/Characters/Player1.fbx";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        AnimationClip idleClip = null;
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip && asset.name == "Idle")
            {
                idleClip = asset as AnimationClip;
                break;
            }
        }
        
        if (idleClip != null)
        {
            idleState.motion = idleClip;
            Debug.Log("Assigned Idle clip to Idle state.");
        }
        else
        {
            Debug.LogWarning("Could not find Idle clip in Player1.fbx.");
        }
        
        // Create Transitions
        // Idle -> Run
        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;
        
        // Run -> Idle
        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;
        
        AssetDatabase.SaveAssets();
        Debug.Log("Successfully created PlayerAnimator and set up states/transitions.");
    }
}