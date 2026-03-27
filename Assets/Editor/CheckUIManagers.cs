using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;

public class CheckUIManagers
{
    public static void Execute()
    {
        string[] sceneGuids = UnityEditor.AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        
        foreach (string guid in sceneGuids)
        {
            string scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            
            // Skip package scenes
            if (scenePath.Contains("Packages/"))
                continue;
            
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            GameObject[] rootObjects = scene.GetRootGameObjects();
            bool foundUIManager = false;
            
            foreach (GameObject obj in rootObjects)
            {
                if (obj.name == "UIManager")
                {
                    Debug.Log($"Found UIManager in scene: {scenePath}");
                    foundUIManager = true;
                    break;
                }
            }
            
            if (!foundUIManager)
            {
                Debug.Log($"No UIManager found in scene: {scenePath}");
            }
        }
    }
}
