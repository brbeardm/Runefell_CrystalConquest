using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AddDebugGroundMarker
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
            DebugGroundMarker marker = player.GetComponent<DebugGroundMarker>();
            if (marker == null)
            {
                marker = player.AddComponent<DebugGroundMarker>();
            }
            
            SerializedObject so = new SerializedObject(marker);
            
            SerializedProperty sizeProp = so.FindProperty("size");
            if (sizeProp != null) sizeProp.floatValue = 0.3f;
            
            SerializedProperty colorProp = so.FindProperty("color");
            if (colorProp != null) colorProp.colorValue = Color.red;
            
            SerializedProperty lineWidthProp = so.FindProperty("lineWidth");
            if (lineWidthProp != null) lineWidthProp.floatValue = 0.05f;
            
            so.ApplyModifiedProperties();
            
            Debug.Log($"Added and configured DebugGroundMarker on {player.name} in {scene.name}");
            EditorUtility.SetDirty(player);
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}