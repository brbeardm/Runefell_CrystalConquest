using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RecreateCampaign
{
    public static void Execute()
    {
        // Step 1: Delete existing asset
        string assetPath = "Assets/_Project/Data/Campaign/DefaultCampaign.asset";
        if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) != null)
        {
            bool deleted = AssetDatabase.DeleteAsset(assetPath);
            Debug.Log(deleted ? $"Deleted {assetPath}" : $"Failed to delete {assetPath}");
        }
        else
        {
            Debug.Log("DefaultCampaign.asset not found — skipping delete.");
        }

        AssetDatabase.Refresh();

        // Step 2: Run the menu command
        CreateDefaultCampaign.Create();

        // Step 3: Save the scene
        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Scene saved: {scene.name}");
    }
}
