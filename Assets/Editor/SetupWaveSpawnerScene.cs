using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SetupWaveSpawnerScene
{
    public static void Execute()
    {
        // Step 1: Create DefaultCampaign asset via the menu command
        CreateDefaultCampaign.Create();
        Debug.Log("Step 1 complete: DefaultCampaign asset created.");

        // Step 2: Make sure we're in the right scene
        var scene = EditorSceneManager.OpenScene("Assets/Gameplay_BridgeRange.unity", OpenSceneMode.Single);

        // Find WaveSpawner
        WaveSpawner spawner = Object.FindAnyObjectByType<WaveSpawner>();
        if (spawner == null)
        {
            Debug.LogError("WaveSpawner not found in scene!");
            return;
        }

        // Use SerializedObject for proper field assignment
        SerializedObject so = new SerializedObject(spawner);

        // Campaign Config
        var campaignAsset = AssetDatabase.LoadAssetAtPath<WaveScalingConfig>("Assets/_Project/Data/Campaign/DefaultCampaign.asset");
        if (campaignAsset == null)
        {
            Debug.LogError("DefaultCampaign.asset not found!");
            return;
        }
        so.FindProperty("campaignConfig").objectReferenceValue = campaignAsset;
        Debug.Log("Assigned campaignConfig.");

        // Orc Prefab
        var orcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab");
        if (orcPrefab == null)
        {
            Debug.LogError("Enemy_Orc.prefab not found!");
            return;
        }
        so.FindProperty("orcPrefab").objectReferenceValue = orcPrefab;
        Debug.Log("Assigned orcPrefab.");

        // Boss Prefabs - set array size to 9, all None
        var bossPrefabsProp = so.FindProperty("bossPrefabs");
        bossPrefabsProp.arraySize = 9;
        for (int i = 0; i < 9; i++)
        {
            bossPrefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = null;
        }
        Debug.Log("Set bossPrefabs array size to 9 (all None).");

        // Spawn Point - find EnemySpawnPoint in scene
        GameObject enemySpawnPoint = GameObject.Find("EnemySpawnPoint");
        if (enemySpawnPoint != null)
        {
            so.FindProperty("spawnPoint").objectReferenceValue = enemySpawnPoint.transform;
            Debug.Log("Assigned spawnPoint to EnemySpawnPoint.");
        }
        else
        {
            Debug.LogWarning("EnemySpawnPoint not found, keeping existing assignment.");
        }

        // Bridge bounds
        so.FindProperty("bridgeMinX").floatValue = -2.4f;
        so.FindProperty("bridgeMaxX").floatValue = 3.2f;
        Debug.Log("Set bridgeMinX=-2.4, bridgeMaxX=3.2.");

        // Spawn Y
        so.FindProperty("spawnY").floatValue = 0.25f;
        Debug.Log("Set spawnY=0.25.");

        // Auto Start
        so.FindProperty("autoStart").boolValue = true;
        Debug.Log("Set autoStart=true.");

        // Initial Delay
        so.FindProperty("initialDelay").floatValue = 3f;
        Debug.Log("Set initialDelay=3.");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);

        // Save the scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Step 2 complete: WaveSpawner configured and scene saved.");
    }
}
