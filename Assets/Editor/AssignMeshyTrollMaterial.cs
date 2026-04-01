using UnityEditor;
using UnityEngine;

public class AssignMeshyTrollMaterial
{
    [MenuItem("Tools/Assign Meshy Troll Material")]
    public static void AssignMaterial()
    {
        // Find the troll in the scene
        GameObject trollRoot = GameObject.Find("Meshy_CrystalTroll");
        if (trollRoot == null)
        {
            Debug.LogError("Meshy_CrystalTroll not found in scene!");
            return;
        }

        // Find the char1 child with SkinnedMeshRenderer
        Transform char1Transform = trollRoot.transform.Find("char1");
        if (char1Transform == null)
        {
            Debug.LogError("char1 not found under Meshy_CrystalTroll!");
            return;
        }

        SkinnedMeshRenderer renderer = char1Transform.GetComponent<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found on char1!");
            return;
        }

        // Load the material
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/Materials/Meshy_CrystalTroll_Material.mat");
        if (mat == null)
        {
            Debug.LogError("Material not found!");
            return;
        }

        // Assign material to renderer
        renderer.material = mat;
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(trollRoot);

        Debug.Log("Meshy Troll Material assigned successfully!");
    }
}
