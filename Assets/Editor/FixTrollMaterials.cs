using UnityEngine;
using UnityEditor;

public class FixTrollMaterials
{
    [MenuItem("Tools/Fix Troll Materials")]
    public static void Execute()
    {
        // Find the Troll_Idle_5meters in the scene
        GameObject troll = GameObject.Find("Troll_Idle_5meters");
        if (troll == null)
        {
            Debug.LogError("Troll_Idle_5meters not found in scene!");
            return;
        }

        // Get the mesh child
        Transform meshTransform = troll.transform.Find("Mesh_0.001");
        if (meshTransform == null)
        {
            Debug.LogError("Mesh_0.001 not found!");
            return;
        }

        SkinnedMeshRenderer renderer = meshTransform.GetComponent<SkinnedMeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found!");
            return;
        }

        // Load the FBX to get its materials
        string fbxPath = "Assets/Characters/Troll_Idle_5meters.fbx";
        Object[] fbxAssets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        
        Debug.Log($"Found {fbxAssets.Length} assets in FBX");
        
        // Find all materials in the FBX
        Material[] fbxMaterials = new Material[fbxAssets.Length];
        int materialCount = 0;
        
        foreach (Object asset in fbxAssets)
        {
            if (asset is Material mat)
            {
                Debug.Log($"Found material in FBX: {mat.name}");
                fbxMaterials[materialCount] = mat;
                materialCount++;
            }
        }

        if (materialCount == 0)
        {
            Debug.LogWarning("No materials found in FBX. Checking for external materials...");
            
            // Try to find materials in the Materials folder
            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Characters/Materials" });
            Material[] externalMaterials = new Material[materialGuids.Length];
            
            for (int i = 0; i < materialGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(materialGuids[i]);
                externalMaterials[i] = AssetDatabase.LoadAssetAtPath<Material>(path);
                Debug.Log($"Found external material: {externalMaterials[i].name}");
            }
            
            if (externalMaterials.Length > 0)
            {
                renderer.materials = externalMaterials;
                Debug.Log($"Assigned {externalMaterials.Length} external materials to Troll");
            }
        }
        else
        {
            System.Array.Resize(ref fbxMaterials, materialCount);
            renderer.materials = fbxMaterials;
            Debug.Log($"Assigned {materialCount} materials from FBX to Troll");
        }

        EditorUtility.SetDirty(renderer);
        Debug.Log("Troll materials fixed!");
    }
}
