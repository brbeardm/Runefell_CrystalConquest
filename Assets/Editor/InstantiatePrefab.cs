using UnityEngine;
using UnityEditor;

public class InstantiatePrefab
{
    [MenuItem("Tools/Instantiate Prefab")]
    public static void Instantiate()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab");
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "CrystalProjectile_Instance";
            Selection.activeGameObject = instance;
            Debug.Log("Prefab instantiated!");
        }
    }
}
