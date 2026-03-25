using UnityEditor;
using UnityEngine;

public class ScaleEnemyPrefabs
{
    public static void Execute()
    {
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
                // Scale down to 50%
                Vector3 currentScale = prefab.transform.localScale;
                prefab.transform.localScale = currentScale * 0.5f;
                
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
                
                Debug.Log($"Scaled {prefab.name} prefab to {prefab.transform.localScale}");
            }
            else
            {
                Debug.LogError($"Could not find enemy prefab at {path}");
            }
        }
    }
}