using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixPlayerShooterOffsetPrefab
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/HeroShooter.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            Transform tier0Base = prefab.transform.Find("Tier0_Base");
            if (tier0Base != null)
            {
                Transform playerBody = tier0Base.Find("PlayerBody");
                if (playerBody != null)
                {
                    Vector3 pos = playerBody.localPosition;
                    playerBody.localPosition = new Vector3(pos.x, pos.y, 0f);
                    Debug.Log($"Fixed PlayerBody offset in prefab. New localPosition: {playerBody.localPosition}");
                }
                
                Transform firePoint = tier0Base.Find("FirePoint");
                if (firePoint != null)
                {
                    // FirePoint was at -3.5 when PlayerBody was at -4. So it should be at 0.5.
                    Vector3 pos = firePoint.localPosition;
                    firePoint.localPosition = new Vector3(pos.x, pos.y, 0.5f);
                    Debug.Log($"Fixed FirePoint offset in prefab. New localPosition: {firePoint.localPosition}");
                }
            }
            
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
        }
        else
        {
            Debug.LogError("Could not find HeroShooter prefab.");
        }
        
        // Also fix CloneShooter prefab
        string clonePrefabPath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject clonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(clonePrefabPath);
        
        if (clonePrefab != null)
        {
            Transform tier0Base = clonePrefab.transform.Find("Tier0_Base");
            if (tier0Base != null)
            {
                Transform playerBody = tier0Base.Find("PlayerBody");
                if (playerBody != null)
                {
                    Vector3 pos = playerBody.localPosition;
                    playerBody.localPosition = new Vector3(pos.x, pos.y, 0f);
                    Debug.Log($"Fixed PlayerBody offset in clone prefab. New localPosition: {playerBody.localPosition}");
                }
                
                Transform firePoint = tier0Base.Find("FirePoint");
                if (firePoint != null)
                {
                    Vector3 pos = firePoint.localPosition;
                    firePoint.localPosition = new Vector3(pos.x, pos.y, 0.5f);
                    Debug.Log($"Fixed FirePoint offset in clone prefab. New localPosition: {firePoint.localPosition}");
                }
            }
            
            EditorUtility.SetDirty(clonePrefab);
            PrefabUtility.SavePrefabAsset(clonePrefab);
        }
        else
        {
            Debug.LogError("Could not find CloneShooter prefab.");
        }
        
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
        
        GameObject firePoint = GameObject.Find("---Player---/Player/Tier0_Base/FirePoint");
        if (firePoint != null)
        {
            Vector3 pos = firePoint.transform.localPosition;
            firePoint.transform.localPosition = new Vector3(pos.x, pos.y, 0.5f);
            
            Debug.Log($"Fixed FirePoint offset in {scene.name}. New localPosition: {firePoint.transform.localPosition}");
            EditorUtility.SetDirty(firePoint);
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}