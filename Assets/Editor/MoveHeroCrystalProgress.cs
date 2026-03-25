using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveHeroCrystalProgress
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
        
        GameObject progress = GameObject.Find("Canvas/HeroCrystalProgress");
        if (progress != null)
        {
            RectTransform rect = progress.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Move it to the right of the PauseButton (which is at X=20, width=60, so ends at X=80)
                // And vertically center it with the PauseButton (PauseButton is at Y=20, height=60, center Y=50)
                // Progress bar height is 40, so Y=30 makes its center Y=50
                rect.anchoredPosition = new Vector2(100f, 30f);
                
                Debug.Log($"Moved HeroCrystalProgress to {rect.anchoredPosition} in {scene.name}");
                EditorUtility.SetDirty(progress);
            }
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}