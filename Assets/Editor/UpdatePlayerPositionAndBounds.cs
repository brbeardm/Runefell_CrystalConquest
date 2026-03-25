using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdatePlayerPositionAndBounds
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
        
        // 1. Update PlayerShooter
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            // Set Z position to -5
            Vector3 pos = player.transform.position;
            player.transform.position = new Vector3(pos.x, pos.y, -5f);
            
            // Update PlayerMover component
            PlayerMover mover = player.GetComponent<PlayerMover>();
            if (mover != null)
            {
                SerializedObject so = new SerializedObject(mover);
                
                SerializedProperty minZProp = so.FindProperty("minZ");
                if (minZProp != null) minZProp.floatValue = -6f;
                
                SerializedProperty maxZProp = so.FindProperty("maxZ");
                if (maxZProp != null) maxZProp.floatValue = -4f;
                
                so.ApplyModifiedProperties();
            }
            
            Debug.Log($"Updated Player start position to Z=-5 and PlayerMover bounds to [-6, -4] in {scene.name}");
            EditorUtility.SetDirty(player);
        }
        
        // 2. Verify ShooterManager
        ShooterManager[] managers = Object.FindObjectsByType<ShooterManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ShooterManager manager in managers)
        {
            SerializedObject so = new SerializedObject(manager);
            
            SerializedProperty zOffsetProp = so.FindProperty("cloneZOffset");
            if (zOffsetProp != null)
            {
                if (zOffsetProp.floatValue != -0.2f)
                {
                    zOffsetProp.floatValue = -0.2f;
                    so.ApplyModifiedProperties();
                    Debug.Log($"Updated ShooterManager cloneZOffset to -0.2 in {scene.name}");
                    EditorUtility.SetDirty(manager);
                }
                else
                {
                    Debug.Log($"ShooterManager cloneZOffset is already -0.2 in {scene.name}");
                }
            }
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}