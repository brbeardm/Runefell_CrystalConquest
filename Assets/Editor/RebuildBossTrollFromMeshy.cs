using UnityEngine;
using UnityEditor;

/// <summary>
/// Rebuilds Boss_TrollBoss prefab using the Troll_Meshy_Base_Idle model.
/// Unpacks nested prefab so all references are flat.
/// Auto-wires WeaponImpactPoint, animator, and all gameplay components.
/// Menu: Tools > Rebuild Boss Troll (Meshy)
/// </summary>
public class RebuildBossTrollFromMeshy
{
    [MenuItem("Tools/Rebuild Boss Troll (Meshy)")]
    public static void Rebuild()
    {
        // Load the visual model prefab
        string meshyPrefabPath = "Assets/_Project/Prefabs/Enemies/Bosses/Troll_Meshy_Base_Idle.prefab";
        GameObject meshyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(meshyPrefabPath);
        if (meshyPrefab == null)
        {
            Debug.LogError($"[RebuildBossTroll] Could not find Meshy prefab at {meshyPrefabPath}");
            return;
        }

        // Load the animator controller
        string controllerPath = "Assets/_Project/Animations/BossTrollController.controller";
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError($"[RebuildBossTroll] Could not find animator controller at {controllerPath}");
            return;
        }

        // Load the enemy data asset
        string dataPath = "Assets/_Project/Data/Enemies/Bosses/Boss_TrollBoss.asset";
        var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(dataPath);
        if (enemyData == null)
        {
            Debug.LogError($"[RebuildBossTroll] Could not find EnemyData at {dataPath}");
            return;
        }

        // Instantiate the Meshy model as the base
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(meshyPrefab);
        go.name = "Boss_TrollBoss";

        // Unpack the nested prefab completely so all objects are flat — no cross-prefab reference issues
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        Debug.Log("[RebuildBossTroll] Nested prefab unpacked");

        // Set layer and tag on root and all children
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        go.layer = enemyLayer;
        go.tag = "Enemy";
        foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = enemyLayer;
        }

        // Reset transform (spawner positions it at runtime)
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        // Wire the animator controller
        Animator animator = go.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            Debug.Log($"[RebuildBossTroll] Animator found on '{animator.gameObject.name}', controller assigned");
        }
        else
        {
            Debug.LogWarning("[RebuildBossTroll] No Animator found on Meshy model — add one manually");
        }

        // Add CapsuleCollider (trigger) on root for boss contact detection
        var capsule = go.AddComponent<CapsuleCollider>();
        capsule.isTrigger = true;
        capsule.center = new Vector3(0f, 0.5f, 0f);
        capsule.radius = 0.3f;
        capsule.height = 1.0f;
        capsule.direction = 1; // Y-axis

        // Add Rigidbody (kinematic, no gravity) for trigger events
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // Add Enemy component and assign data
        var enemy = go.AddComponent<Enemy>();
        var enemySO = new SerializedObject(enemy);
        enemySO.FindProperty("data").objectReferenceValue = enemyData;
        enemySO.ApplyModifiedProperties();

        // Add BossHealthDisplay
        var healthDisplay = go.AddComponent<BossHealthDisplay>();
        var healthSO = new SerializedObject(healthDisplay);
        healthSO.FindProperty("yOffset").floatValue = 4f;
        healthSO.FindProperty("textColor").colorValue = Color.red;
        healthSO.FindProperty("fontSize").intValue = 40;
        healthSO.ApplyModifiedProperties();

        // Add TrollBossBehavior
        var trollBehavior = go.AddComponent<TrollBossBehavior>();
        var trollSO = new SerializedObject(trollBehavior);
        trollSO.FindProperty("knockbackForce").floatValue = 5f;
        trollSO.FindProperty("knockbackRadius").floatValue = 5f;
        trollSO.ApplyModifiedProperties();

        // Auto-wire WeaponImpactPoint (search all children by name)
        GameObject impactPoint = null;
        foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "WeaponImpactPoint")
            {
                impactPoint = child.gameObject;
                break;
            }
        }

        if (impactPoint != null)
        {
            var so = new SerializedObject(trollBehavior);
            var prop = so.FindProperty("weaponImpactPoint");
            if (prop != null)
            {
                prop.objectReferenceValue = impactPoint;
                so.ApplyModifiedProperties();
                Debug.Log($"[RebuildBossTroll] WeaponImpactPoint found and wired!");
            }
        }
        else
        {
            Debug.LogWarning("[RebuildBossTroll] WeaponImpactPoint NOT FOUND — add it to Troll_Meshy_Base_Idle prefab under the hammer, then rebuild.");
        }

        // Save as prefab, overwriting the old Boss_TrollBoss
        string outputPath = "Assets/_Project/Prefabs/Enemies/Bosses/Boss_TrollBoss.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, outputPath);
        Object.DestroyImmediate(go);

        Debug.Log($"[RebuildBossTroll] Boss_TrollBoss prefab rebuilt at {outputPath}");
        AssetDatabase.Refresh();
    }
}
