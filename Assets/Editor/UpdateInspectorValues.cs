using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdateInspectorValues
{
    public static void Execute()
    {
        // Update in current scene (Gameplay_BridgeRange)
        UpdateScene("Assets/Scenes/Gameplay_BridgeRange.unity");
        
        // Update in Gameplay scene
        UpdateScene("Assets/Scenes/Gameplay.unity");
        
        // Reopen the original scene
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
    }

    private static void UpdateScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        // Update ShooterManager
        ShooterManager[] managers = Object.FindObjectsByType<ShooterManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ShooterManager manager in managers)
        {
            SerializedObject so = new SerializedObject(manager);
            
            SerializedProperty spacingProp = so.FindProperty("cloneXSpacing");
            if (spacingProp != null) spacingProp.floatValue = 0.25f;
            
            SerializedProperty zOffsetProp = so.FindProperty("cloneZOffset");
            if (zOffsetProp != null) zOffsetProp.floatValue = -0.2f;
            
            so.ApplyModifiedProperties();
            Debug.Log($"Updated ShooterManager values in {scene.name}");
            EditorUtility.SetDirty(manager);
        }
        
        // Update PlayerMover
        PlayerMover[] movers = Object.FindObjectsByType<PlayerMover>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (PlayerMover mover in movers)
        {
            SerializedObject so = new SerializedObject(mover);
            
            SerializedProperty minZProp = so.FindProperty("minZ");
            if (minZProp != null) minZProp.floatValue = -1f;
            
            so.ApplyModifiedProperties();
            Debug.Log($"Updated PlayerMover minZ to -1 in {scene.name}");
            EditorUtility.SetDirty(mover);
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}