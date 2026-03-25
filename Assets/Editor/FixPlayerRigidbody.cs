using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixPlayerRigidbody
{
    public static void Execute()
    {
        // 1. Update PlayerShooter in scene
        UpdateScene("Assets/Scenes/Gameplay_BridgeRange.unity");
        UpdateScene("Assets/Scenes/Gameplay.unity");
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
        
        // 2. Update CloneShooter prefab
        UpdateClonePrefab();
    }

    private static void UpdateScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Must be false for trigger detection with kinematic enemies
                rb.useGravity = false;
                // Freeze all constraints so physics doesn't move the player
                rb.constraints = RigidbodyConstraints.FreezeAll;
                
                EditorUtility.SetDirty(player);
                Debug.Log($"Fixed Player Rigidbody in {scene.name}");
            }
        }
        
        EditorSceneManager.SaveScene(scene);
    }
    
    private static void UpdateClonePrefab()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Must be false for trigger detection with kinematic enemies
                rb.useGravity = false;
                // Freeze all constraints so physics doesn't move the clone
                rb.constraints = RigidbodyConstraints.FreezeAll;
                
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                Debug.Log("Fixed CloneShooter Rigidbody.");
            }
        }
    }
}