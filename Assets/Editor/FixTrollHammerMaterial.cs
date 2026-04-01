using UnityEngine;
using UnityEditor;

public class FixTrollHammerMaterial
{
    [MenuItem("Tools/Fix Troll Hammer Material")]
    public static void Execute()
    {
        // Find the hammer mesh
        GameObject hammer = GameObject.Find("Meshy_AI_TrollHammer_0331035215_texture");
        if (hammer == null)
        {
            Debug.LogError("Hammer mesh not found!");
            return;
        }

        MeshRenderer renderer = hammer.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer not found on hammer!");
            return;
        }

        // Try to load the TrollHammer material first
        Material trollHammerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/Weapons/TrollHammer.mat");
        
        if (trollHammerMat != null)
        {
            Debug.Log("Found TrollHammer.mat, assigning to hammer mesh");
            renderer.material = trollHammerMat;
            EditorUtility.SetDirty(renderer);
            Debug.Log("Troll hammer material fixed!");
            return;
        }

        // If TrollHammer.mat doesn't exist or isn't configured, create a new material
        Debug.LogWarning("TrollHammer.mat not found or not properly configured. Creating new material...");
        
        Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        newMat.name = "TrollHammer_Material";

        // Load textures
        Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture.png");
        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_normal.png");
        Texture2D metallic = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_metallic.png");
        Texture2D roughness = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_roughness.png");

        if (diffuse != null)
        {
            newMat.SetTexture("_BaseMap", diffuse);
            Debug.Log("Assigned diffuse texture");
        }
        else
        {
            Debug.LogWarning("Diffuse texture not found!");
        }

        if (normal != null)
        {
            newMat.SetTexture("_BumpMap", normal);
            newMat.SetFloat("_BumpScale", 1.0f);
            Debug.Log("Assigned normal texture");
        }

        if (metallic != null)
        {
            newMat.SetTexture("_MetallicGlossMap", metallic);
            Debug.Log("Assigned metallic texture");
        }

        if (roughness != null)
        {
            // Roughness is typically stored in the alpha channel of metallic map
            // But we can also use it separately if needed
            Debug.Log("Roughness texture available: " + roughness.name);
        }

        renderer.material = newMat;
        EditorUtility.SetDirty(renderer);
        Debug.Log("Troll hammer material created and assigned!");
    }
}
