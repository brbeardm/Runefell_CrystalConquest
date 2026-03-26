using UnityEditor;
using UnityEngine;

public class RecreateCampaignAndAssign
{
    [MenuItem("Tools/Recreate Campaign And Assign")]
    public static void Execute()
    {
        string path = "Assets/_Project/Data/Campaign/DefaultCampaign.asset";

        // Step 1: Delete existing asset
        if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null)
        {
            bool deleted = AssetDatabase.DeleteAsset(path);
            Debug.Log(deleted ? $"Deleted existing asset at {path}" : $"Failed to delete asset at {path}");
        }
        else
        {
            Debug.Log("No existing DefaultCampaign.asset found, skipping delete.");
        }

        AssetDatabase.Refresh();

        // Step 2: Invoke the Runefell menu item to recreate it
        CreateDefaultCampaign.Create();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Step 3: Load the new asset and assign to WaveSpawner
        var newAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        if (newAsset == null)
        {
            Debug.LogError($"Could not load new DefaultCampaign.asset at {path}");
            return;
        }

        GameObject waveSpawnerGO = GameObject.Find("WaveSpawner");
        if (waveSpawnerGO == null)
        {
            Debug.LogError("WaveSpawner not found in scene.");
            return;
        }

        WaveSpawner waveSpawner = waveSpawnerGO.GetComponent<WaveSpawner>();
        if (waveSpawner == null)
        {
            Debug.LogError("WaveSpawner component not found.");
            return;
        }

        var so = new SerializedObject(waveSpawner);
        var prop = so.FindProperty("campaignConfig");
        if (prop == null)
        {
            Debug.LogError("campaignConfig property not found on WaveSpawner.");
            return;
        }

        prop.objectReferenceValue = newAsset;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(waveSpawner);
        Debug.Log($"Assigned new DefaultCampaign.asset to WaveSpawner.campaignConfig.");

        // Step 4: Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scene saved.");
    }
}
