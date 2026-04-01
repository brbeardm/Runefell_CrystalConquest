using UnityEngine;
using UnityEditor;

public class DiagnoseBossProjectileDamage
{
    public static void Execute()
    {
        Debug.Log("=== BOSS PROJECTILE DAMAGE DIAGNOSIS ===");

        // Find the Boss Orc in the scene
        var bossOrc = GameObject.Find("---Targets---/CrystalBlock");
        if (bossOrc == null)
        {
            Debug.LogError("Could not find Boss Orc in scene!");
            return;
        }

        Debug.Log($"[Diagnosis] Found Boss Orc: {bossOrc.name}");
        Debug.Log($"[Diagnosis] Boss Orc Layer: {LayerMask.LayerToName(bossOrc.layer)} (index: {bossOrc.layer})");

        // Check for Enemy component
        var enemy = bossOrc.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"[Diagnosis] Boss has Enemy component: YES");
            Debug.Log($"[Diagnosis] Boss Current Health: {enemy.CurrentHealth}/{enemy.MaxHealth}");
        }
        else
        {
            Debug.LogWarning("[Diagnosis] Boss has Enemy component: NO - This is the problem!");
        }

        // Check for colliders
        var colliders = bossOrc.GetComponentsInChildren<Collider>();
        Debug.Log($"[Diagnosis] Boss has {colliders.Length} colliders");
        foreach (var col in colliders)
        {
            Debug.Log($"  - {col.gameObject.name}: Layer={LayerMask.LayerToName(col.gameObject.layer)}, IsTrigger={col.isTrigger}");
        }

        // Check collision matrix
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        
        Debug.Log($"[Diagnosis] Projectile Layer: {projectileLayer}");
        Debug.Log($"[Diagnosis] Enemy Layer: {enemyLayer}");

        bool canCollide = !Physics.GetIgnoreLayerCollision(projectileLayer, enemyLayer);
        Debug.Log($"[Diagnosis] Projectile-Enemy collision enabled: {canCollide}");

        if (!canCollide)
        {
            Debug.LogError("[Diagnosis] PROBLEM FOUND: Projectile and Enemy layers are set to ignore each other!");
        }

        // Check if projectile prefab exists
        var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Gameplay/CrystalProjectile.prefab");
        if (projectilePrefab != null)
        {
            Debug.Log($"[Diagnosis] Projectile prefab found");
            Debug.Log($"[Diagnosis] Projectile Layer: {LayerMask.LayerToName(projectilePrefab.layer)} (index: {projectilePrefab.layer})");
            
            var pooledProj = projectilePrefab.GetComponent<PooledProjectile>();
            if (pooledProj != null)
            {
                Debug.Log($"[Diagnosis] Projectile Damage: {pooledProj.Damage}");
            }
        }
        else
        {
            Debug.LogWarning("[Diagnosis] Could not find projectile prefab");
        }

        Debug.Log("=== END DIAGNOSIS ===");
    }
}
