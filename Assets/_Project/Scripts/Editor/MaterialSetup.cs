using UnityEngine;
using UnityEditor;

public class MaterialSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Troll Materials")]
    static void SetupTrollMaterials()
    {
        // --- Troll Material ---
        // Try both possible material names
        string[] matPaths = {
            "Assets/Characters/Materials/Material.001 1.mat",
            "Assets/Characters/Materials/Material.001.mat"
        };

        // Try both possible texture locations
        string[] diffusePaths = {
            "Assets/Characters/Image_0.jpg",
            "Assets/Characters/Troll_Idle_5meters.fbm/Image_0.jpg",
            "Assets/Characters/Textures/Image_0.jpg"
        };
        string[] normalPaths = {
            "Assets/Characters/Image_2.jpg",
            "Assets/Characters/Troll_Idle_5meters.fbm/Image_2.jpg",
            "Assets/Characters/Textures/Image_2.jpg"
        };

        Texture2D diffuse = FindFirst(diffusePaths);
        Texture2D normal = FindFirst(normalPaths);

        foreach (string matPath in matPaths)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null) continue;

            if (diffuse != null)
            {
                mat.SetTexture("_BaseMap", diffuse);
                mat.SetTexture("_MainTex", diffuse);
                Debug.Log($"Assigned diffuse texture to {matPath}");
            }
            if (normal != null)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", normal);
                Debug.Log($"Assigned normal texture to {matPath}");
            }
            EditorUtility.SetDirty(mat);
        }

        if (diffuse == null)
            Debug.LogError("Could not find diffuse texture (Image_0.jpg) anywhere!");
        if (normal == null)
            Debug.LogWarning("Could not find normal texture (Image_2.jpg)");

        // --- Also try to fix any Skinned Mesh Renderer in the scene ---
        var smrs = Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
        foreach (var smr in smrs)
        {
            if (smr.gameObject.name.Contains("Troll") || smr.gameObject.name.Contains("Mesh_0"))
            {
                Material sceneMat = smr.sharedMaterial;
                if (sceneMat != null && diffuse != null)
                {
                    sceneMat.SetTexture("_BaseMap", diffuse);
                    sceneMat.SetTexture("_MainTex", diffuse);
                    if (normal != null)
                    {
                        sceneMat.EnableKeyword("_NORMALMAP");
                        sceneMat.SetTexture("_BumpMap", normal);
                    }
                    EditorUtility.SetDirty(sceneMat);
                    Debug.Log($"Fixed material on scene object: {smr.gameObject.name}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Done! Troll should now be textured.");
    }

    static Texture2D FindFirst(string[] paths)
    {
        foreach (string path in paths)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                Debug.Log($"Found texture: {path}");
                return tex;
            }
        }
        return null;
    }
}
