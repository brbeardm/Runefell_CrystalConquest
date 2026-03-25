using UnityEngine;
using UnityEditor;

public class InspectShooterPrefabs
{
    [MenuItem("Tools/Inspect Shooter Prefabs")]
    public static void Inspect()
    {
        string[] paths = {
            "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab",
            "Assets/_Project/Prefabs/Shooters/HeroShooter.prefab"
        };

        foreach (string path in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                Debug.Log($"--- {prefab.name} ---");
                foreach (Transform child in prefab.transform)
                {
                    Debug.Log($"Child: {child.name}, LocalPos: {child.localPosition}");
                }
                CapsuleCollider col = prefab.GetComponent<CapsuleCollider>();
                if (col != null)
                {
                    Debug.Log($"Collider Center: {col.center}, Height: {col.height}");
                }
            }
        }
    }
}
