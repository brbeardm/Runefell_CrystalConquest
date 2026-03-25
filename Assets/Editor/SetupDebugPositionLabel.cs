using UnityEngine;
using UnityEditor;

public class SetupDebugPositionLabel
{
    [MenuItem("Tools/Setup Debug Position Label")]
    public static void Setup()
    {
        // 1. Setup Player in Scene
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            DebugPositionLabel playerLabel = player.GetComponent<DebugPositionLabel>();
            if (playerLabel == null)
            {
                playerLabel = player.AddComponent<DebugPositionLabel>();
            }

            SerializedObject so = new SerializedObject(playerLabel);
            so.Update();
            so.FindProperty("yOffset").floatValue = 1.5f;
            so.FindProperty("color").colorValue = Color.yellow;
            so.FindProperty("fontSize").intValue = 14;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(player);
            Debug.Log("Added DebugPositionLabel to Player in scene.");
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }

        // 2. Setup CloneShooter Prefab
        string prefabPath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject clonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (clonePrefab != null)
        {
            DebugPositionLabel cloneLabel = clonePrefab.GetComponent<DebugPositionLabel>();
            if (cloneLabel == null)
            {
                cloneLabel = clonePrefab.AddComponent<DebugPositionLabel>();
            }

            SerializedObject so = new SerializedObject(cloneLabel);
            so.Update();
            so.FindProperty("yOffset").floatValue = 1.5f;
            so.FindProperty("color").colorValue = Color.green;
            so.FindProperty("fontSize").intValue = 14;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(clonePrefab);
            PrefabUtility.SavePrefabAsset(clonePrefab);
            Debug.Log("Added DebugPositionLabel to CloneShooter prefab.");
        }
        else
        {
            Debug.LogError("CloneShooter prefab not found!");
        }
    }
}
