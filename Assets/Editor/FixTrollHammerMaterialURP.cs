using UnityEngine;
using UnityEditor;

public class FixTrollHammerMaterialURP
{
    [MenuItem("Tools/Fix Troll Hammer Material URP")]
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

        // Create a new material with URP Lit shader
        Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        newMat.name = "TrollHammer_URP";

        // Load textures from the Weapons folder
        Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture.png");
        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_normal.png");
        Texture2D metallic = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_metallic.png");
        Texture2D roughness = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Characters/Weapons/Meshy_AI_TrollHammer_0331035215_texture_roughness.png");

        Debug.Log("Loading textures...");
        if (diffuse != null)
        {
            newMat.SetTexture("_BaseMap", diffuse);
            Debug.Log("✓ Assigned diffuse texture: " + diffuse.name);
        }
        else
        {
            Debug.LogError("✗ Diffuse texture not found!");
        }

        if (normal != null)
        {
            newMat.SetTexture("_BumpMap", normal);
            newMat.SetFloat("_BumpScale", 1.0f);
            Debug.Log("✓ Assigned normal texture: " + normal.name);
        }
        else
        {
            Debug.LogWarning("✗ Normal texture not found!");
        }

        if (metallic != null)
        {
            newMat.SetTexture("_MetallicGlossMap", metallic);
            newMat.SetFloat("_Metallic", 1.0f);
            Debug.Log("✓ Assigned metallic texture: " + metallic.name);
        }
        else
        {
            Debug.LogWarning("✗ Metallic texture not found!");
        }

        if (roughness != null)
        {
            Debug.Log("✓ Roughness texture available: " + roughness.name);
        }

        // Set base color to white so textures show properly
        newMat.SetColor("_BaseColor", Color.white);

        // Apply the material
        renderer.material = newMat;
        EditorUtility.SetDirty(renderer);
        
        Debug.Log("✓ Troll hammer material fixed with URP shader!");
    }
}
