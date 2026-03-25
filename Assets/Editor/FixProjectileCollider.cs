using UnityEngine;
using UnityEditor;

public class FixProjectileCollider
{
    [MenuItem("Tools/Fix Projectile Collider")]
    public static void Execute()
    {
        string path = "Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError("Prefab not found: " + path); return; }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            var root = scope.prefabContentsRoot;

            // Fix SphereCollider — make it large enough to reliably detect hits
            var col = root.GetComponent<SphereCollider>();
            if (col != null)
            {
                float oldRadius = col.radius;
                // World scale is (0.05, 0.05, 0.20) — radius 0.1 = 0.005 world units (too small)
                // Set radius to 1.0 so world radius = 0.05 units (10x bigger, still visually tight)
                col.radius = 1.0f;
                Debug.Log($"SphereCollider radius: {oldRadius} -> {col.radius} (world radius: {col.radius * root.transform.lossyScale.x})");
            }

            // Ensure Rigidbody collision detection is ContinuousDynamic for best trigger detection
            var rb = root.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                Debug.Log($"Rigidbody collisionDetection set to ContinuousDynamic");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Projectile collider fixed.");
    }
}
