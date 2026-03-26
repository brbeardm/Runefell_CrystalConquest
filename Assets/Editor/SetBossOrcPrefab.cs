using UnityEditor;
using UnityEngine;

public static class SetBossOrcPrefab
{
    public static void Execute()
    {
        GameObject orcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab");
        if (orcPrefab == null)
        {
            Debug.LogError("Enemy_Orc.prefab not found!");
            return;
        }

        SetOrcPrefabOnBoss("Assets/_Project/Prefabs/Enemies/Bosses/Boss_CrystalSorcerer.prefab",
            typeof(CrystalSorcererBossBehavior), orcPrefab);

        SetOrcPrefabOnBoss("Assets/_Project/Prefabs/Enemies/Bosses/Boss_CrystalNecromancer.prefab",
            typeof(CrystalNecromancerBossBehavior), orcPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Done — orcPrefab set on both Crystal Sorcerer and Crystal Necromancer bosses.");
    }

    private static void SetOrcPrefabOnBoss(string prefabPath, System.Type behaviorType, GameObject orcPrefab)
    {
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabAsset == null)
        {
            Debug.LogError($"Prefab not found: {prefabPath}");
            return;
        }

        // Load prefab contents for editing
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);

        Component behavior = root.GetComponent(behaviorType);
        if (behavior == null)
        {
            Debug.LogError($"{behaviorType.Name} not found on {prefabPath}");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        SerializedObject so = new SerializedObject(behavior);
        SerializedProperty orcProp = so.FindProperty("orcPrefab");
        if (orcProp == null)
        {
            Debug.LogError($"'orcPrefab' property not found on {behaviorType.Name}");
            PrefabUtility.UnloadPrefabContents(root);
            return;
        }

        orcProp.objectReferenceValue = orcPrefab;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);

        Debug.Log($"Set orcPrefab = Enemy_Orc on {System.IO.Path.GetFileName(prefabPath)}");
    }
}
