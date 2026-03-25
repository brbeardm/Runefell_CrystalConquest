using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetupGroundLayerGameplay
{
    public static void Execute()
    {
        string scenePath = "Assets/Scenes/Gameplay.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        GameObject bridgeFloor = GameObject.Find("---World---/BridgeFloor");
        if (bridgeFloor != null)
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            bridgeFloor.layer = groundLayer;
            Debug.Log("Assigned 'Ground' layer to " + bridgeFloor.name + " in Gameplay scene");
            EditorSceneManager.SaveScene(scene);
        }
        else
        {
            Debug.Log("Could not find '---World---/BridgeFloor' GameObject in Gameplay scene.");
        }
        
        // Reopen the original scene
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
    }
}