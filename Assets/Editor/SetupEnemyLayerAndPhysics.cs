using UnityEditor;
using UnityEngine;

public class SetupEnemyLayerAndPhysics
{
    public static void Execute()
    {
        // 1. Create "Enemy" layer
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        bool layerExists = false;
        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
            if (layerSP.stringValue == "Enemy")
            {
                layerExists = true;
                break;
            }
        }
        if (!layerExists)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerSP.stringValue))
                {
                    layerSP.stringValue = "Enemy";
                    tagManager.ApplyModifiedProperties();
                    break;
                }
            }
        }

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int defaultLayer = LayerMask.NameToLayer("Default");

        if (enemyLayer == -1)
        {
            Debug.LogError("Failed to create or find Enemy layer.");
            return;
        }

        // 2. Assign layers and triggers to prefabs
        string[] enemyPrefabPaths = new string[]
        {
            "Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab",
            "Assets/_Project/Prefabs/Enemies/Enemy_SkeletonWarrior.prefab",
            "Assets/_Project/Prefabs/Enemies/Enemy_Troll.prefab",
            "Assets/_Project/Prefabs/Enemies/Enemy_Balrog.prefab",
            "Assets/_Project/Prefabs/Enemies/Enemy_Sauron.prefab"
        };

        foreach (string path in enemyPrefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                SetLayerRecursively(prefab, enemyLayer);
                Collider[] colliders = prefab.GetComponentsInChildren<Collider>(true);
                foreach (var col in colliders)
                {
                    col.isTrigger = true;
                }
                EditorUtility.SetDirty(prefab);
            }
        }

        string[] otherEnemyLayerPaths = new string[]
        {
            "Assets/_Project/Prefabs/HeroCrystal/HeroCrystal.prefab",
            "Assets/_Project/Prefabs/CrystalBalls/CrystalBall.prefab"
        };

        foreach (string path in otherEnemyLayerPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                SetLayerRecursively(prefab, enemyLayer);
                EditorUtility.SetDirty(prefab);
            }
        }

        // 3. Setup CrystalProjectile
        GameObject projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab");
        if (projPrefab != null)
        {
            Collider[] colliders = projPrefab.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
            Rigidbody rb = projPrefab.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = false;
            }
            EditorUtility.SetDirty(projPrefab);
        }

        // 4. Update Physics collision matrix
        if (projectileLayer != -1 && defaultLayer != -1)
        {
            Physics.IgnoreLayerCollision(projectileLayer, defaultLayer, true);
        }
        if (projectileLayer != -1 && enemyLayer != -1)
        {
            Physics.IgnoreLayerCollision(projectileLayer, enemyLayer, false);
        }

        // Save DynamicsManager
        Object[] dynamicsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset");
        if (dynamicsAssets != null && dynamicsAssets.Length > 0)
        {
            EditorUtility.SetDirty(dynamicsAssets[0]);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Setup complete.");
    }

    private static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}