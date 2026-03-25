using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupPlayerMoverGroundMask
{
    public static void Execute()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer == -1)
        {
            Debug.LogError("Ground layer not found!");
            return;
        }

        int groundMask = 1 << groundLayer;

        // Update in current scene (Gameplay_BridgeRange)
        UpdateScene("Assets/Scenes/Gameplay_BridgeRange.unity", groundMask);
        
        // Update in Gameplay scene
        UpdateScene("Assets/Scenes/Gameplay.unity", groundMask);
        
        // Reopen the original scene
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
    }

    private static void UpdateScene(string scenePath, int groundMask)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        PlayerMover[] movers = Object.FindObjectsByType<PlayerMover>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (PlayerMover mover in movers)
        {
            SerializedObject so = new SerializedObject(mover);
            SerializedProperty groundMaskProp = so.FindProperty("groundMask");
            if (groundMaskProp != null)
            {
                groundMaskProp.intValue = groundMask;
                so.ApplyModifiedProperties();
                Debug.Log($"Updated groundMask on {mover.gameObject.name} in {scene.name}");
                EditorUtility.SetDirty(mover);
            }
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}