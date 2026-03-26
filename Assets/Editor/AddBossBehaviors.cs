using UnityEditor;
using UnityEngine;

public static class AddBossBehaviors
{
    public static void Execute()
    {
        var bossMap = new (string prefabName, System.Type behaviorType)[]
        {
            ("Boss_OrcBoss",            typeof(OrcBossBehavior)),
            ("Boss_TrollBoss",          typeof(TrollBossBehavior)),
            ("Boss_WraithBoss",         typeof(WraithBossBehavior)),
            ("Boss_SorcererBoss",       typeof(SorcererBossBehavior)),
            ("Boss_CrystalOrc",         typeof(CrystalOrcBossBehavior)),
            ("Boss_CrystalTroll",       typeof(CrystalTrollBossBehavior)),
            ("Boss_CrystalWraith",      typeof(CrystalWraithBossBehavior)),
            ("Boss_CrystalSorcerer",    typeof(CrystalSorcererBossBehavior)),
            ("Boss_CrystalNecromancer", typeof(CrystalNecromancerBossBehavior)),
        };

        foreach (var (prefabName, behaviorType) in bossMap)
        {
            string path = $"Assets/_Project/Prefabs/Enemies/Bosses/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found: {path}");
                continue;
            }

            // Open prefab for editing
            string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
            GameObject root = PrefabUtility.LoadPrefabContents(prefabAssetPath);

            // Check if behavior already exists
            if (root.GetComponent(behaviorType) == null)
            {
                root.AddComponent(behaviorType);
                Debug.Log($"Added {behaviorType.Name} to {prefabName}");
            }
            else
            {
                Debug.Log($"{behaviorType.Name} already exists on {prefabName}");
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabAssetPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All boss behaviors added!");
    }
}
