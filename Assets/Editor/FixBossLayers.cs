using UnityEditor;
using UnityEngine;

public static class FixBossLayers
{
    public static void Execute()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer < 0)
        {
            Debug.LogError("'Enemy' layer not found!");
            return;
        }

        string[] bossPrefabNames = new string[]
        {
            "Boss_OrcBoss",
            "Boss_TrollBoss",
            "Boss_WraithBoss",
            "Boss_SorcererBoss",
            "Boss_CrystalOrc",
            "Boss_CrystalTroll",
            "Boss_CrystalWraith",
            "Boss_CrystalSorcerer",
            "Boss_CrystalNecromancer",
        };

        foreach (var name in bossPrefabNames)
        {
            string path = $"Assets/_Project/Prefabs/Enemies/Bosses/{name}.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                Debug.LogError($"Could not load prefab: {path}");
                continue;
            }

            // Set layer on root and all children
            SetLayerRecursive(root, enemyLayer);

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"Set layer to Enemy on {name}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("All boss prefabs set to Enemy layer.");
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
