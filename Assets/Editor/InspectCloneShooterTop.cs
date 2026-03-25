using UnityEngine;
using UnityEditor;

public class InspectCloneShooterTop
{
    [MenuItem("Tools/Inspect Clone Shooter Top")]
    public static void Execute()
    {
        string path = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError("Prefab not found"); return; }

        Debug.Log("=== CloneShooter direct children ===");
        foreach (Transform child in prefab.transform)
        {
            var mr = child.GetComponent<MeshRenderer>();
            var smr = child.GetComponent<SkinnedMeshRenderer>();
            string matInfo = "";
            if (mr != null)
            {
                matInfo = " [MeshRenderer]";
                foreach (var m in mr.sharedMaterials)
                    matInfo += $" mat={m?.name} color={m?.color}";
            }
            if (smr != null) matInfo += " [SkinnedMeshRenderer]";

            // Count all renderers in children
            var allMR = child.GetComponentsInChildren<MeshRenderer>(true);
            var allSMR = child.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (allMR.Length > 0 || allSMR.Length > 0)
                matInfo += $" (children: {allMR.Length} MR, {allSMR.Length} SMR)";

            Debug.Log($"  Child: '{child.name}'{matInfo}");
        }
    }
}
