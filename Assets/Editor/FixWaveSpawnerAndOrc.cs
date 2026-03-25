using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixWaveSpawnerAndOrc
{
    public static void Execute()
    {
        // 1. Fix WaveSpawner in scenes
        UpdateScene("Assets/Scenes/Gameplay_BridgeRange.unity");
        UpdateScene("Assets/Scenes/Gameplay.unity");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
        
        // 2. Fix Enemy_Orc Prefab
        FixOrcPrefab();
    }

    private static void UpdateScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        WaveSpawner spawner = Object.FindAnyObjectByType<WaveSpawner>();
        if (spawner != null)
        {
            SerializedObject so = new SerializedObject(spawner);
            
            // Check spawnPoint
            SerializedProperty spawnPointProp = so.FindProperty("spawnPoint");
            if (spawnPointProp != null)
            {
                GameObject spawnObj = GameObject.Find("EnemySpawnPoint");
                if (spawnObj == null)
                {
                    spawnObj = new GameObject("EnemySpawnPoint");
                    spawnObj.transform.position = new Vector3(0f, 0.25f, 15f);
                }
                else
                {
                    spawnObj.transform.position = new Vector3(0f, 0.25f, 15f);
                }
                
                spawnPointProp.objectReferenceValue = spawnObj.transform;
            }
            
            // Set Spawn Y and Fallback Spawn Z
            SerializedProperty spawnYProp = so.FindProperty("spawnY");
            if (spawnYProp != null) spawnYProp.floatValue = 0.25f;
            
            SerializedProperty fallbackZProp = so.FindProperty("fallbackSpawnZ");
            if (fallbackZProp != null) fallbackZProp.floatValue = 15f;
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
            Debug.Log($"Fixed WaveSpawner in {scene.name}");
        }
        
        EditorSceneManager.SaveScene(scene);
    }
    
    private static void FixOrcPrefab()
    {
        string prefabPath = "Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError("Could not find Enemy_Orc prefab.");
            return;
        }
        
        // 2. COLLIDER
        Collider[] oldColliders = prefab.GetComponents<Collider>();
        foreach (Collider c in oldColliders)
        {
            Object.DestroyImmediate(c, true);
        }
        
        CapsuleCollider cap = prefab.AddComponent<CapsuleCollider>();
        cap.isTrigger = true;
        // The orc model is scaled to 0.5, and the root is scaled to 0.5.
        // Let's set the collider to fit a standard humanoid.
        cap.center = new Vector3(0f, 1f, 0f);
        cap.radius = 0.4f;
        cap.height = 2f;
        
        // 3. TAG
        prefab.tag = "Enemy";
        
        // 4. ENEMY COMPONENT
        Enemy enemyComp = prefab.GetComponent<Enemy>();
        if (enemyComp == null) enemyComp = prefab.AddComponent<Enemy>();
        
        SerializedObject enemySo = new SerializedObject(enemyComp);
        SerializedProperty dataProp = enemySo.FindProperty("data");
        if (dataProp != null)
        {
            EnemyData orcData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Orc.asset");
            dataProp.objectReferenceValue = orcData;
        }
        enemySo.ApplyModifiedProperties();
        
        // 5. RIGIDBODY
        Rigidbody rb = prefab.GetComponent<Rigidbody>();
        if (rb == null) rb = prefab.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // 6. FACING & 7. SCALE
        Transform fbxChild = prefab.transform.Find("enemy_orc");
        if (fbxChild != null)
        {
            fbxChild.localPosition = Vector3.zero;
            fbxChild.localRotation = Quaternion.Euler(0f, 180f, 0f);
            
            // The prompt says: "The orc should be small enough that projectiles at their current flight height can hit the collider. 
            // If the orc is too short, scale it up slightly until the collider overlaps with the projectile flight path."
            // Let's set it to (1, 1, 1) so the total scale is 0.5 (since root is 0.5).
            // If we set it to (0.5, 0.5, 0.5), total scale is 0.25, which might be too small.
            // Let's use (1, 1, 1) for the child, so the root scale of 0.5 makes it half size.
            fbxChild.localScale = Vector3.one;
        }
        
        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("Fixed Enemy_Orc prefab.");
    }
}