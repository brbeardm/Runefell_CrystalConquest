using UnityEngine;
using UnityEditor;

public class InspectCloneShooter
{
    [MenuItem("Tools/Inspect Clone Shooter")]
    public static void Execute()
    {
        string path = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) { Debug.LogError("Prefab not found"); return; }

        Debug.Log($"=== CloneShooter children ===");
        foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
        {
            string indent = new string(' ', GetDepth(t, prefab.transform) * 2);
            var mr = t.GetComponent<MeshRenderer>();
            string matInfo = "";
            if (mr != null)
            {
                matInfo = " [MeshRenderer]";
                foreach (var m in mr.sharedMaterials)
                    matInfo += $" mat={m?.name}({m?.shader?.name})";
            }
            var sr = t.GetComponent<SkinnedMeshRenderer>();
            if (sr != null) matInfo += " [SkinnedMeshRenderer]";
            var cap = t.GetComponent<CapsuleCollider>();
            if (cap != null) matInfo += " [CapsuleCollider]";

            Debug.Log($"{indent}{t.name} layer={LayerMask.LayerToName(t.gameObject.layer)}{matInfo}");
        }
    }

    static int GetDepth(Transform t, Transform root)
    {
        int d = 0;
        while (t != root && t != null) { t = t.parent; d++; }
        return d;
    }
}
