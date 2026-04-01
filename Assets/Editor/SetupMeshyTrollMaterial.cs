using UnityEditor;
using UnityEngine;

public class SetupMeshyTrollMaterial
{
    [MenuItem("Tools/Setup Meshy Troll Material")]
    public static void SetupMaterial()
    {
        // Load the material
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/Materials/Meshy_CrystalTroll_Material.mat");
        if (mat == null)
        {
            Debug.LogError("Material not found!");
            return;
        }

        // Load textures
        Texture2D baseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0.png");
        Texture2D normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0_normal.png");
        Texture2D metallicTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0_metallic.png");
        Texture2D roughnessTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Troll_Meshy/Meshy_AI_CrystalTroll_biped_texture_0_roughness.png");

        if (baseTexture == null || normalTexture == null || metallicTexture == null || roughnessTexture == null)
        {
            Debug.LogError("One or more textures not found!");
            return;
        }

        // Assign textures to material
        mat.SetTexture("_BaseMap", baseTexture);
        mat.SetTexture("_MainTex", baseTexture);
        mat.SetTexture("_BumpMap", normalTexture);
        mat.SetTexture("_MetallicGlossMap", metallicTexture);

        // Enable normal map keyword
        mat.EnableKeyword("_NORMALMAP");

        // Set material properties for PBR
        mat.SetFloat("_Metallic", 1.0f);
        mat.SetFloat("_Smoothness", 0.5f);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        Debug.Log("Meshy Troll Material setup complete!");
    }
}
