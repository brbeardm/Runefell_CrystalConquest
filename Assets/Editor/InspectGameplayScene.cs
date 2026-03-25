using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InspectGameplayScene
{
    public static void Execute()
    {
        string scenePath = "Assets/Scenes/Gameplay.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject go in rootObjects)
        {
            Debug.Log("Root: " + go.name);
            if (go.name == "---World---")
            {
                foreach (Transform child in go.transform)
                {
                    Debug.Log("  Child: " + child.name);
                }
            }
        }
        
        // Reopen the original scene
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
    }
}