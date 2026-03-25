using UnityEngine;
using UnityEditor;

public class FixProjectileColliderFinal
{
    [MenuItem("Tools/Fix Projectile Collider Final")]
    public static void Execute()
    {
        string path = "Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab";

        using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            var root = scope.prefabContentsRoot;

            // The projectile scale is (0.05, 0.05, 0.20).
            // We need the world-space sphere to be large enough to not tunnel.
            // Enemy capsule world radius = 0.2. Projectile speed = 35 u/s.
            // At 50Hz physics, projectile moves 0.7 units/step.
            // World radius needs to be > 0.35 to guarantee overlap detection.
            // Scale X = 0.05, so local radius = 0.35 / 0.05 = 7.0
            var col = root.GetComponent<SphereCollider>();
            if (col != null)
            {
                col.radius = 7f;
                Debug.Log($"SphereCollider radius set to {col.radius} (world radius = {col.radius * root.transform.localScale.x})");
            }

            // ContinuousSpeculative is the ONLY mode that prevents tunneling
            // for trigger colliders in Unity. ContinuousDynamic only works for
            // non-trigger colliders against static geometry.
            var rb = root.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                Debug.Log($"Rigidbody set to ContinuousSpeculative");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Done — projectile collider and collision detection fixed.");
    }
}
