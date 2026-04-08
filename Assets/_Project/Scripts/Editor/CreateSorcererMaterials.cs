using UnityEngine;
using UnityEditor;

public static class CreateSorcererMaterials
{
    [MenuItem("Tools/Create Sorcerer URP Materials")]
    static void Create()
    {
        CreateMaterial(
            "Assets/Characters/Sorcerer_Meshy/Sorcerer_URP.mat",
            "Assets/Characters/Sorcerer_Meshy/Sorcerer_texture_0.png",
            "Assets/Characters/Sorcerer_Meshy/Sorcerer_texture_0_normal.png",
            "Assets/Characters/Sorcerer_Meshy/Sorcerer_texture_0_metallic.png",
            "Assets/Characters/Sorcerer_Meshy/Sorcerer_texture_0_roughness.png",
            "Sorcerer_URP"
        );

        CreateMaterial(
            "Assets/Characters/Weapons/Sorcerer_Mace/Sorcerer_Mace_URP.mat",
            "Assets/Characters/Weapons/Sorcerer_Mace/Sorcerer_Mace_texture.png",
            "Assets/Characters/Weapons/Sorcerer_Mace/Sorcerer_Mace_texture_normal.png",
            "Assets/Characters/Weapons/Sorcerer_Mace/Sorcerer_Mace_texture_metallic.png",
            "Assets/Characters/Weapons/Sorcerer_Mace/Sorcerer_Mace_texture_roughness.png",
            "Sorcerer_Mace_URP"
        );

        // Also assign to any Sorcerer renderers in the scene
        AssignToSceneRenderers();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateSorcererMaterials] Done! Both materials created and assigned.");
    }

    static void CreateMaterial(string matPath, string albedoPath, string normalPath, string metallicPath, string roughnessPath, string matName)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("[CreateSorcererMaterials] URP Lit shader not found!");
            return;
        }

        var mat = new Material(shader);
        mat.name = matName;

        // Albedo
        var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        if (albedo != null)
        {
            mat.SetTexture("_BaseMap", albedo);
            mat.SetTexture("_MainTex", albedo);
            Debug.Log($"[CreateSorcererMaterials] Albedo: {albedoPath}");
        }
        else
        {
            Debug.LogWarning($"[CreateSorcererMaterials] Albedo not found: {albedoPath}");
        }

        // Normal map
        var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        if (normal != null)
        {
            mat.EnableKeyword("_NORMALMAP");
            mat.SetTexture("_BumpMap", normal);
            Debug.Log($"[CreateSorcererMaterials] Normal: {normalPath}");
        }

        // Metallic map
        var metallic = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
        if (metallic != null)
        {
            mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            mat.SetTexture("_MetallicGlossMap", metallic);
            mat.SetFloat("_Metallic", 1f);
            Debug.Log($"[CreateSorcererMaterials] Metallic: {metallicPath}");
        }

        // Smoothness from roughness (URP uses smoothness = 1 - roughness)
        // URP Lit reads smoothness from the metallic map's alpha channel by default
        // Set smoothness to a reasonable value since we can't invert the roughness map here
        mat.SetFloat("_Smoothness", 0.3f);

        // Enable emission for shield VFX
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.black);

        AssetDatabase.CreateAsset(mat, matPath);
        Debug.Log($"[CreateSorcererMaterials] Created material: {matPath}");
    }

    static void AssignToSceneRenderers()
    {
        var sorcererMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/Sorcerer_Meshy/Sorcerer_URP.mat");
        var maceMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/Weapons/Sorcerer_Mace/Sorcerer_Mace_URP.mat");

        if (sorcererMat == null || maceMat == null)
        {
            Debug.LogWarning("[CreateSorcererMaterials] Materials not yet created, skipping scene assignment");
            return;
        }

        // Find all renderers in scene
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var r in renderers)
        {
            string objName = r.gameObject.name.ToLower();
            string parentName = r.transform.parent != null ? r.transform.parent.gameObject.name.ToLower() : "";

            // Match sorcerer body mesh
            if (objName.Contains("sorcerer") && !objName.Contains("mace") ||
                parentName.Contains("sorcerer") && !objName.Contains("mace") && objName.Contains("char"))
            {
                r.sharedMaterial = sorcererMat;
                EditorUtility.SetDirty(r);
                Debug.Log($"[CreateSorcererMaterials] Assigned Sorcerer_URP to: {r.gameObject.name}");
            }

            // Match mace mesh
            if (objName.Contains("mace") || objName.Contains("sorcerer_mace"))
            {
                r.sharedMaterial = maceMat;
                EditorUtility.SetDirty(r);
                Debug.Log($"[CreateSorcererMaterials] Assigned Sorcerer_Mace_URP to: {r.gameObject.name}");
            }
        }
    }
}
