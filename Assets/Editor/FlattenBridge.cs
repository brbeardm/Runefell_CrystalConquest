using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlattenBridge
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
        
        GameObject bridgeFloor = GameObject.Find("---World---/BridgeFloor");
        if (bridgeFloor != null)
        {
            // Ensure rotation is zeroed out so it's completely flat
            bridgeFloor.transform.localRotation = Quaternion.identity;
            
            // Ensure the Y scale is small enough to be a flat surface, but keep X and Z
            Vector3 scale = bridgeFloor.transform.localScale;
            bridgeFloor.transform.localScale = new Vector3(scale.x, 0.5f, scale.z);
            
            // Ensure the Y position is constant (0)
            Vector3 pos = bridgeFloor.transform.localPosition;
            bridgeFloor.transform.localPosition = new Vector3(pos.x, 0f, pos.z);
            
            Debug.Log($"Flattened bridge in {scene.name}");
            EditorUtility.SetDirty(bridgeFloor);
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}