using UnityEngine;
using UnityEditor;

public class CheckEnemyHitDetection
{
    [MenuItem("Tools/Check Enemy Hit Detection")]
    public static void Execute()
    {
        // Check layer indices
        Debug.Log("=== Layer Indices ===");
        for (int i = 0; i < 32; i++)
        {
            string name = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(name))
                Debug.Log($"  Layer {i}: '{name}'");
        }

        // Check collision matrix between key layers
        Debug.Log("=== Collision Matrix (relevant pairs) ===");
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        int defaultLayer = 0;

        Debug.Log($"Projectile layer: {projectileLayer}");
        Debug.Log($"Enemy layer: {enemyLayer}");
        Debug.Log($"Player layer: {playerLayer}");

        if (projectileLayer >= 0 && enemyLayer >= 0)
        {
            bool projVsEnemy = !Physics.GetIgnoreLayerCollision(projectileLayer, enemyLayer);
            Debug.Log($"Projectile({projectileLayer}) vs Enemy({enemyLayer}): {(projVsEnemy ? "ENABLED" : "DISABLED ← BUG")}");
        }

        if (projectileLayer >= 0)
        {
            bool projVsDefault = !Physics.GetIgnoreLayerCollision(projectileLayer, defaultLayer);
            Debug.Log($"Projectile({projectileLayer}) vs Default(0): {(projVsDefault ? "ENABLED" : "DISABLED")}");
        }

        // Check the enemy prefab collider details
        Debug.Log("=== Enemy_Orc Prefab Collider ===");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab");
        if (prefab != null)
        {
            var cols = prefab.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
                Debug.Log($"  [{c.gameObject.name}] layer={LayerMask.LayerToName(c.gameObject.layer)}({c.gameObject.layer}) isTrigger={c.isTrigger} type={c.GetType().Name}");

            var rbs = prefab.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rbs)
                Debug.Log($"  [{rb.gameObject.name}] isKinematic={rb.isKinematic}");
        }

        // Check projectile prefab
        Debug.Log("=== CrystalProjectile Prefab ===");
        var proj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab");
        if (proj != null)
        {
            var cols = proj.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
                Debug.Log($"  [{c.gameObject.name}] layer={LayerMask.LayerToName(c.gameObject.layer)}({c.gameObject.layer}) isTrigger={c.isTrigger} radius={((c is SphereCollider sc) ? sc.radius.ToString() : "n/a")} scale={c.transform.lossyScale}");

            var rbs = proj.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rb in rbs)
                Debug.Log($"  [{rb.gameObject.name}] isKinematic={rb.isKinematic} collisionDetection={rb.collisionDetectionMode}");
        }
    }
}
