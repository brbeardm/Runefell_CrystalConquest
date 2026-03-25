using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixPlayerShooterOffset
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
        
        GameObject playerBody = GameObject.Find("---Player---/Player/Tier0_Base/PlayerBody");
        if (playerBody != null)
        {
            // The mesh sits about 1 unit behind (negative Z) from the transform origin.
            // We need to move the mesh child object FORWARD so that the model's center/feet align with the prefab's origin (0,0,0).
            // Currently localPosition is (0, 0.5, -4). Wait, -4?
            // Let's check the parent's position.
            // The prompt says "The player's visible mesh/model is offset from the prefab's pivot point — the mesh sits about 1 unit behind (negative Z) from the transform origin."
            // So we should set Z to 0.
            
            Vector3 pos = playerBody.transform.localPosition;
            playerBody.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
            
            Debug.Log($"Fixed PlayerBody offset in {scene.name}. New localPosition: {playerBody.transform.localPosition}");
            EditorUtility.SetDirty(playerBody);
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}