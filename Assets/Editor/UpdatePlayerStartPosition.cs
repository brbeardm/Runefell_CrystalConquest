using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdatePlayerStartPosition
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
        
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            // Update Z position to -1
            Vector3 pos = player.transform.position;
            player.transform.position = new Vector3(pos.x, pos.y, -1f);
            
            // Update PlayerMover component
            PlayerMover mover = player.GetComponent<PlayerMover>();
            if (mover != null)
            {
                SerializedObject so = new SerializedObject(mover);
                
                SerializedProperty minZProp = so.FindProperty("minZ");
                if (minZProp != null) minZProp.floatValue = -5f;
                
                SerializedProperty maxZProp = so.FindProperty("maxZ");
                if (maxZProp != null) maxZProp.floatValue = 6f;
                
                so.ApplyModifiedProperties();
            }
            
            Debug.Log($"Updated Player start position and PlayerMover bounds in {scene.name}");
            EditorUtility.SetDirty(player);
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}