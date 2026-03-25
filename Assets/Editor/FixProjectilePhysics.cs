using UnityEditor;
using UnityEngine;

public class FixProjectilePhysics
{
    public static void Execute()
    {
        // 1. Fix CrystalProjectile Prefab
        string projPath = "Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab";
        GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(projPath);
        if (projPrefab != null)
        {
            projPrefab.layer = LayerMask.NameToLayer("Projectile");
            Rigidbody rb = projPrefab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = false;
            }
            EditorUtility.SetDirty(projPrefab);
            Debug.Log("Checked and fixed CrystalProjectile prefab (Layer, Rigidbody).");
        }

        // 2 & 4. Fix Physics Collision Matrix
        int projLayer = LayerMask.NameToLayer("Projectile");
        int defaultLayer = LayerMask.NameToLayer("Default");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (projLayer != -1)
        {
            if (defaultLayer != -1) Physics.IgnoreLayerCollision(projLayer, defaultLayer, true); // Ignore Default
            if (enemyLayer != -1) Physics.IgnoreLayerCollision(projLayer, enemyLayer, false); // Collide with Enemy
            Physics.IgnoreLayerCollision(projLayer, projLayer, true); // Ignore Projectile vs Projectile
            
            Object[] dynamicsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset");
            if (dynamicsAssets != null && dynamicsAssets.Length > 0)
            {
                EditorUtility.SetDirty(dynamicsAssets[0]);
            }
            Debug.Log("Checked and fixed Physics Collision Matrix (Proj vs Proj = Ignore, Proj vs Default = Ignore, Proj vs Enemy = Collide).");
        }

        // 3. Fix Player Layer
        GameObject playerRoot = GameObject.Find("---Player---");
        if (playerRoot != null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer == -1) playerLayer = defaultLayer;
            
            SetLayerRecursively(playerRoot, playerLayer);
            Debug.Log("Checked and fixed Player layer to " + LayerMask.LayerToName(playerLayer));
        }
        
        AssetDatabase.SaveAssets();
    }

    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}