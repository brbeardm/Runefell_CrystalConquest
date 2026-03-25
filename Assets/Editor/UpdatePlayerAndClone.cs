using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpdatePlayerAndClone
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
            // Tag
            if (player.tag != "Player")
            {
                player.tag = "Player";
            }
            
            // CapsuleCollider
            CapsuleCollider cap = player.GetComponent<CapsuleCollider>();
            if (cap == null)
            {
                cap = player.AddComponent<CapsuleCollider>();
                cap.center = new Vector3(0f, 0.5f, 0f);
                cap.radius = 0.5f;
                cap.height = 1f;
            }
            cap.isTrigger = true;
            
            // Rigidbody
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = player.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            
            EditorUtility.SetDirty(player);
            Debug.Log($"Updated PlayerShooter in {scene.name}");
        }
        
        EditorSceneManager.SaveScene(scene);
    }
    
    private static void UpdateClonePrefab()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            // Tag
            if (prefab.tag != "Player")
            {
                prefab.tag = "Player";
            }
            
            // CapsuleCollider
            CapsuleCollider cap = prefab.GetComponent<CapsuleCollider>();
            if (cap == null)
            {
                cap = prefab.AddComponent<CapsuleCollider>();
                cap.center = new Vector3(0f, 0.5f, 0f);
                cap.radius = 0.5f;
                cap.height = 1f;
            }
            cap.isTrigger = true;
            
            // Rigidbody
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = prefab.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("Updated CloneShooter prefab.");
        }
        else
        {
            Debug.LogError("Could not find CloneShooter prefab.");
        }
    }
}