using UnityEngine;
using UnityEditor;

public class RebuildTrollPrefab
{
    [MenuItem("Tools/Rebuild Troll Boss Prefab")]
    static void Rebuild()
    {
        // Load the new model
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Troll_New_TPose2.fbx");
        if (model == null)
        {
            Debug.LogError("Could not find Troll_New_TPose2.fbx");
            return;
        }

        // Load the old prefab to read the EnemyData reference
        var oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/_Project/Prefabs/Enemies/Bosses/Enemy_Boss_Troll.prefab");
        ScriptableObject enemyData = null;
        if (oldPrefab != null)
        {
            var oldEnemy = oldPrefab.GetComponent("Enemy");
            if (oldEnemy != null)
            {
                var so = new SerializedObject(oldEnemy);
                var dataProp = so.FindProperty("data");
                if (dataProp != null)
                    enemyData = dataProp.objectReferenceValue as ScriptableObject;
            }
        }

        // Instantiate the new model
        var go = Object.Instantiate(model);
        go.name = "Enemy_Boss_Troll";
        go.tag = "Enemy";
        go.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        // Shift all children up so feet sit on the bridge
        foreach (Transform child in go.transform)
            child.localPosition += new Vector3(0f, 50f, 0f);

        // Animator
        var animator = go.GetComponent<Animator>();
        if (animator == null)
            animator = go.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
            "Assets/_Project/Animations/BossTrollController.controller");
        // Avatar should already be set from the FBX import
        animator.applyRootMotion = false;

        // CapsuleCollider
        var col = go.AddComponent<CapsuleCollider>();
        col.radius = 0.5f;
        col.height = 2f;
        col.center = new Vector3(0f, 1f, 0f);
        col.direction = 1; // Y-axis
        col.isTrigger = false;

        // Rigidbody
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.useGravity = true;
        rb.isKinematic = true;

        // Enemy script
        var enemyComp = go.AddComponent(System.Type.GetType("Enemy, Assembly-CSharp"));
        if (enemyComp != null && enemyData != null)
        {
            var so = new SerializedObject(enemyComp);
            var dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                dataProp.objectReferenceValue = enemyData;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // TrollBossBehavior script
        var troll = go.AddComponent(System.Type.GetType("TrollBossBehavior, Assembly-CSharp"));
        if (troll != null)
        {
            var so = new SerializedObject(troll);
            so.FindProperty("intimidationTravelDistance").floatValue = 25f;
            so.FindProperty("intimidationPauseTime").floatValue = 0.5f;
            so.FindProperty("intimidationAttackTime").floatValue = 1.5f;
            so.FindProperty("roarPauseTime").floatValue = 0.3f;
            so.FindProperty("roarDuration").floatValue = 1f;
            so.FindProperty("intimidationKillRadius").floatValue = 3f;
            so.FindProperty("deathStrikeRange").floatValue = 2f;
            so.FindProperty("deathStrikeTime").floatValue = 1f;
            so.FindProperty("deathStrikePause").floatValue = 1.5f;
            so.FindProperty("dyingDuration").floatValue = 2f;
            so.FindProperty("knockbackForce").floatValue = 5f;
            so.FindProperty("knockbackRadius").floatValue = 5f;
            var colorProp = so.FindProperty("stompColor");
            if (colorProp != null)
                colorProp.colorValue = new Color(0.4f, 0.7f, 0.2f, 0.6f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // BossHealthDisplay script
        var health = go.AddComponent(System.Type.GetType("BossHealthDisplay, Assembly-CSharp"));
        if (health != null)
        {
            var so = new SerializedObject(health);
            so.FindProperty("yOffset").floatValue = 3f;
            so.FindProperty("textColor").colorValue = new Color(1f, 0f, 0f, 1f);
            so.FindProperty("fontSize").intValue = 40;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Save as prefab, replacing the old one
        string path = "Assets/_Project/Prefabs/Enemies/Bosses/Enemy_Boss_Troll.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        Debug.Log("Troll Boss prefab rebuilt from Troll_New_TPose2 at " + path);
    }
}
