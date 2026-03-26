using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AssignBossPrefabs
{
    public static void Execute()
    {
        // Make sure we're in the right scene
        var scene = EditorSceneManager.OpenScene("Assets/Gameplay_BridgeRange.unity", OpenSceneMode.Single);

        WaveSpawner spawner = Object.FindAnyObjectByType<WaveSpawner>();
        if (spawner == null)
        {
            Debug.LogError("WaveSpawner not found in scene!");
            return;
        }

        // Boss prefab paths in order (index 0-8)
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

        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty bossPrefabsProp = so.FindProperty("bossPrefabs");
        bossPrefabsProp.arraySize = 9;

        for (int i = 0; i < bossPrefabNames.Length; i++)
        {
            string path = $"Assets/_Project/Prefabs/Enemies/Bosses/{bossPrefabNames[i]}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"Could not load prefab at: {path}");
                continue;
            }
            bossPrefabsProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
            Debug.Log($"Assigned Element {i}: {bossPrefabNames[i]}");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("WaveSpawner boss prefabs assigned and scene saved!");
    }
}
