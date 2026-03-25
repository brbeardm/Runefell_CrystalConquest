using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupShooterManagerSpacing
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
        
        ShooterManager[] managers = Object.FindObjectsByType<ShooterManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ShooterManager manager in managers)
        {
            SerializedObject so = new SerializedObject(manager);
            SerializedProperty spacingProp = so.FindProperty("cloneXSpacing");
            if (spacingProp != null)
            {
                spacingProp.floatValue = 0.15f;
                so.ApplyModifiedProperties();
                Debug.Log($"Updated cloneXSpacing to 0.15 on {manager.gameObject.name} in {scene.name}");
                EditorUtility.SetDirty(manager);
            }
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}