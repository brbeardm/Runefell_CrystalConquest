using UnityEngine;
using UnityEditor;

public static class CreateBossPrefabs
{
    private struct BossInfo
    {
        public string name;
        public string behaviorType; // Component type name
        public Color color;
        public float healthRegenRate;
    }

    [MenuItem("Runefell/Create Boss Prefabs and Data")]
    public static void Create()
    {
        var bosses = new BossInfo[]
        {
            new BossInfo { name = "Boss_OrcBoss",             behaviorType = "OrcBossBehavior",             color = new Color(0.4f, 0.6f, 0.2f) },
            new BossInfo { name = "Boss_TrollBoss",           behaviorType = "TrollBossBehavior",           color = new Color(0.3f, 0.5f, 0.3f) },
            new BossInfo { name = "Boss_WraithBoss",          behaviorType = "WraithBossBehavior",          color = new Color(0.6f, 0.6f, 0.8f) },
            new BossInfo { name = "Boss_SorcererBoss",        behaviorType = "SorcererBossBehavior",        color = new Color(0.5f, 0.2f, 0.8f) },
            new BossInfo { name = "Boss_CrystalOrc",          behaviorType = "CrystalOrcBossBehavior",      color = new Color(0.3f, 0.8f, 1.0f) },
            new BossInfo { name = "Boss_CrystalTroll",        behaviorType = "CrystalTrollBossBehavior",    color = new Color(0.2f, 1.0f, 0.5f), healthRegenRate = 5f },
            new BossInfo { name = "Boss_CrystalWraith",       behaviorType = "CrystalWraithBossBehavior",   color = new Color(0.7f, 0.7f, 1.0f) },
            new BossInfo { name = "Boss_CrystalSorcerer",     behaviorType = "CrystalSorcererBossBehavior", color = new Color(1.0f, 0.3f, 0.8f) },
            new BossInfo { name = "Boss_CrystalNecromancer",  behaviorType = "CrystalNecromancerBossBehavior", color = new Color(0.3f, 0.0f, 0.3f) },
        };

        // Ensure directories exist
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Enemies/Bosses"))
            AssetDatabase.CreateFolder("Assets/_Project/Data/Enemies", "Bosses");
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/Enemies/Bosses"))
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs/Enemies", "Bosses");

        for (int i = 0; i < bosses.Length; i++)
        {
            var info = bosses[i];

            // --- Create EnemyData ScriptableObject ---
            var data = ScriptableObject.CreateInstance<EnemyData>();
            data.displayName = info.name.Replace("Boss_", "").Replace("Boss", " Boss");
            data.isBoss = true;
            data.isInstantKill = true;
            data.maxHealth = 200; // placeholder, overridden at runtime by WaveScalingConfig
            data.moveSpeed = 1f;  // placeholder, overridden at runtime
            data.contactDamage = 999;
            data.scaleMultiplier = 5f;
            data.scoreValue = 100 + (i * 50);
            data.healthRegenRate = info.healthRegenRate;

            string dataPath = $"Assets/_Project/Data/Enemies/Bosses/{info.name}.asset";
            AssetDatabase.CreateAsset(data, dataPath);

            // --- Create Boss Prefab ---
            // Start with a capsule primitive
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = info.name;

            // Scale
            go.transform.localScale = Vector3.one * 5f;

            // Set color on material
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", info.color);
                renderer.sharedMaterial = mat;

                string matPath = $"Assets/_Project/Prefabs/Enemies/Bosses/Mat_{info.name}.mat";
                AssetDatabase.CreateAsset(mat, matPath);
            }

            // Configure collider as trigger
            var capsuleCol = go.GetComponent<CapsuleCollider>();
            if (capsuleCol != null)
                capsuleCol.isTrigger = true;

            // Add Rigidbody (kinematic)
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Add Enemy component and assign data
            var enemy = go.AddComponent<Enemy>();
            var so = new SerializedObject(enemy);
            so.FindProperty("data").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Add BossBehavior subclass
            var behaviorType = System.Type.GetType(info.behaviorType);
            if (behaviorType != null)
                go.AddComponent(behaviorType);
            else
                Debug.LogWarning($"Could not find type: {info.behaviorType}");

            // Add BossHealthDisplay
            go.AddComponent<BossHealthDisplay>();

            // Set tag (must exist in project)
            go.tag = "Enemy";

            // Save as prefab
            string prefabPath = $"Assets/_Project/Prefabs/Enemies/Bosses/{info.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

            // Clean up scene object
            Object.DestroyImmediate(go);

            Debug.Log($"Created boss: {info.name} at {prefabPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All 9 boss prefabs and data assets created!");
    }
}
