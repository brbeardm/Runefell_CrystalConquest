using UnityEditor;
using UnityEngine;

public class ScaleCloneShooter
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            // Scale down to 50%
            Vector3 currentScale = prefab.transform.localScale;
            prefab.transform.localScale = currentScale * 0.5f;
            
            // The collider is a CapsuleCollider on the root.
            // Since we are scaling the root transform, the collider will automatically scale with it.
            // We don't need to manually adjust the collider's radius or height unless we want to change its relative size.
            // The prompt says "Make sure the collider still works at the new size."
            // Scaling the transform is the standard way to scale an object and its colliders in Unity.
            
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            
            Debug.Log($"Scaled CloneShooter prefab to {prefab.transform.localScale}");
        }
        else
        {
            Debug.LogError("Could not find CloneShooter prefab.");
        }
    }
}