using UnityEngine;
using UnityEditor;

public class DiagnoseBossCollision
{
    public static void Execute()
    {
        Debug.Log("=== BOSS COLLISION DIAGNOSIS ===");

        // Load the Boss Orc prefab
        var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Bosses/Enemy_Boss_Orc.prefab");
        if (bossPrefab == null)
        {
            Debug.LogError("Could not load Boss Orc prefab!");
            return;
        }

        Debug.Log($"[Diagnosis] Boss Orc Prefab loaded: {bossPrefab.name}");
        Debug.Log($"[Diagnosis] Boss Orc Layer: {LayerMask.LayerToName(bossPrefab.layer)} (index: {bossPrefab.layer})");

        // Check for Enemy component
        var enemy = bossPrefab.GetComponent<Enemy>();
        Debug.Log($"[Diagnosis] Boss has Enemy component: {(enemy != null ? "YES" : "NO")}");

        // Check all colliders on the boss and its children
        var allColliders = bossPrefab.GetComponentsInChildren<Collider>();
        Debug.Log($"[Diagnosis] Total colliders found: {allColliders.Length}");
        
        foreach (var col in allColliders)
        {
            Debug.Log($"  Collider: {col.gameObject.name}");
            Debug.Log($"    - Type: {col.GetType().Name}");
            Debug.Log($"    - Layer: {LayerMask.LayerToName(col.gameObject.layer)} (index: {col.gameObject.layer})");
            Debug.Log($"    - IsTrigger: {col.isTrigger}");
            Debug.Log($"    - Enabled: {col.enabled}");
            
            // Check if this collider's gameobject has an Enemy component
            var colEnemy = col.GetComponent<Enemy>();
            if (colEnemy != null)
            {
                Debug.Log($"    - Has Enemy component: YES");
            }
            else
            {
                var parentEnemy = col.GetComponentInParent<Enemy>();
                Debug.Log($"    - Has Enemy component (parent): {(parentEnemy != null ? "YES" : "NO")}");
            }
        }

        // Check the projectile prefab
        var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Gameplay/CrystalProjectile.prefab");
        if (projectilePrefab != null)
        {
            Debug.Log($"[Diagnosis] Projectile Prefab: {projectilePrefab.name}");
            var projCollider = projectilePrefab.GetComponent<Collider>();
            if (projCollider != null)
            {
                Debug.Log($"  - Collider Type: {projCollider.GetType().Name}");
                Debug.Log($"  - IsTrigger: {projCollider.isTrigger}");
                Debug.Log($"  - Layer: {LayerMask.LayerToName(projectilePrefab.layer)} (index: {projectilePrefab.layer})");
            }
        }

        Debug.Log("=== END DIAGNOSIS ===");
    }
}
