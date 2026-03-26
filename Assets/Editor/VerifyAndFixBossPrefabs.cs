using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class VerifyAndFixBossPrefabs
{
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Gameplay_BridgeRange.unity", OpenSceneMode.Single);

        WaveSpawner spawner = Object.FindAnyObjectByType<WaveSpawner>();
        if (spawner == null)
        {
            Debug.LogError("WaveSpawner not found!");
            return;
        }

        SerializedObject so = new SerializedObject(spawner);
        SerializedProperty bossProp = so.FindProperty("bossPrefabs");

        Debug.Log($"bossPrefabs array size: {bossProp.arraySize}");
        for (int i = 0; i < bossProp.arraySize; i++)
        {
            var elem = bossProp.GetArrayElementAtIndex(i).objectReferenceValue;
            Debug.Log($"  Element {i}: {(elem != null ? elem.name : "NULL")}");
        }

        // If any are null or wrong, re-assign
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

        bool needsFix = bossProp.arraySize != 9;
        if (!needsFix)
        {
            for (int i = 0; i < 9; i++)
            {
                var elem = bossProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (elem == null || elem.name != bossPrefabNames[i])
                {
                    needsFix = true;
                    break;
                }
            }
        }

        if (needsFix)
        {
            Debug.Log("Re-assigning boss prefabs...");
            bossProp.arraySize = 9;
            for (int i = 0; i < bossPrefabNames.Length; i++)
            {
                string path = $"Assets/_Project/Prefabs/Enemies/Bosses/{bossPrefabNames[i]}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError($"Prefab not found: {path}");
                    continue;
                }
                bossProp.GetArrayElementAtIndex(i).objectReferenceValue = prefab;
                Debug.Log($"  Assigned Element {i}: {bossPrefabNames[i]}");
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Boss prefabs re-assigned and scene saved.");
        }
        else
        {
            Debug.Log("All boss prefabs already correctly assigned. No changes needed.");
        }
    }
}
