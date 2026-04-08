using UnityEngine;
using UnityEditor;

public static class SetupSorcererPrefab
{
    [MenuItem("Tools/Setup Sorcerer Prefab Components")]
    public static void Setup()
    {
        string prefabPath = "Assets/_Project/Prefabs/Enemies/Bosses/Sorcerer.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("[SetupSorcererPrefab] Sorcerer.prefab not found at " + prefabPath);
            return;
        }

        // Open prefab for editing
        string assetPath = AssetDatabase.GetAssetPath(prefab);
        GameObject root = PrefabUtility.LoadPrefabContents(assetPath);

        // Set tag and layer
        root.tag = "Enemy";
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        SetLayerRecursive(root, enemyLayer);

        // ── Enemy component ──
        var enemy = root.GetComponent<Enemy>();
        if (enemy == null)
            enemy = root.AddComponent<Enemy>();

        // Assign EnemyData
        var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/Data/Enemies/Bosses/Boss_SorcererBoss.asset");
        if (enemyData != null)
        {
            var so = new SerializedObject(enemy);
            var dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                dataProp.objectReferenceValue = enemyData;
                so.ApplyModifiedProperties();
                Debug.Log("[SetupSorcererPrefab] EnemyData assigned: Boss_SorcererBoss");
            }
        }
        else
        {
            Debug.LogWarning("[SetupSorcererPrefab] Boss_SorcererBoss.asset not found!");
        }

        // ── SorcererBossBehavior ──
        var behavior = root.GetComponent<SorcererBossBehavior>();
        if (behavior == null)
            behavior = root.AddComponent<SorcererBossBehavior>();

        // Wire up weaponImpactPoint
        var impactPoint = FindChildRecursive(root.transform, "WeaponImpactPoint");
        if (impactPoint != null)
        {
            var bso = new SerializedObject(behavior);
            var wipProp = bso.FindProperty("weaponImpactPoint");
            if (wipProp != null)
            {
                wipProp.objectReferenceValue = impactPoint.gameObject;
                bso.ApplyModifiedProperties();
                Debug.Log("[SetupSorcererPrefab] WeaponImpactPoint wired up");
            }
        }
        else
        {
            Debug.LogWarning("[SetupSorcererPrefab] WeaponImpactPoint not found in hierarchy!");
        }

        // ── BossHealthDisplay ──
        var healthDisplay = root.GetComponent<BossHealthDisplay>();
        if (healthDisplay == null)
            healthDisplay = root.AddComponent<BossHealthDisplay>();

        // ── CapsuleCollider ──
        var collider = root.GetComponent<CapsuleCollider>();
        if (collider == null)
            collider = root.AddComponent<CapsuleCollider>();
        collider.isTrigger = true;
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0f, 0.5f, 0f);

        // ── Rigidbody ──
        var rb = root.GetComponent<Rigidbody>();
        if (rb == null)
            rb = root.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        PrefabUtility.UnloadPrefabContents(root);

        Debug.Log("[SetupSorcererPrefab] Sorcerer prefab setup complete!");
        Debug.Log("  - Enemy + EnemyData (Boss_SorcererBoss)");
        Debug.Log("  - SorcererBossBehavior + WeaponImpactPoint");
        Debug.Log("  - BossHealthDisplay");
        Debug.Log("  - CapsuleCollider (trigger, h=2, center=(0, 0.5, 0))");
        Debug.Log("  - Rigidbody (kinematic)");
        Debug.Log("  - Layer: Enemy, Tag: Enemy");
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
            child.gameObject.layer = layer;
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }
        return null;
    }
}
