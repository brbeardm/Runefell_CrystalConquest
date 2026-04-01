using UnityEngine;
using UnityEditor;

public class VerifyHammerTextures
{
    [MenuItem("Tools/Verify Hammer Textures")]
    public static void Execute()
    {
        GameObject hammer = GameObject.Find("Meshy_AI_TrollHammer_0331035215_texture");
        if (hammer == null)
        {
            Debug.LogError("Hammer mesh not found!");
            return;
        }

        MeshRenderer renderer = hammer.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer not found!");
            return;
        }

        Material mat = renderer.sharedMaterial;
        if (mat == null)
        {
            Debug.LogError("No material assigned!");
            return;
        }

        Debug.Log("=== HAMMER MATERIAL INFO ===");
        Debug.Log($"Material Name: {mat.name}");
        Debug.Log($"Shader: {mat.shader.name}");

        Debug.Log("\n=== TEXTURE ASSIGNMENTS ===");
        
        // Check all texture properties
        Texture baseMap = mat.GetTexture("_BaseMap");
        Debug.Log($"_BaseMap: {(baseMap != null ? baseMap.name : "NOT ASSIGNED")}");

        Texture bumpMap = mat.GetTexture("_BumpMap");
        Debug.Log($"_BumpMap (Normal): {(bumpMap != null ? bumpMap.name : "NOT ASSIGNED")}");

        Texture metallicMap = mat.GetTexture("_MetallicGlossMap");
        Debug.Log($"_MetallicGlossMap: {(metallicMap != null ? metallicMap.name : "NOT ASSIGNED")}");

        Debug.Log("\n=== MATERIAL PROPERTIES ===");
        Debug.Log($"_Metallic: {mat.GetFloat("_Metallic")}");
        Debug.Log($"_BumpScale: {mat.GetFloat("_BumpScale")}");
        Debug.Log($"_BaseColor: {mat.GetColor("_BaseColor")}");

        // List all available textures in the Weapons folder
        Debug.Log("\n=== AVAILABLE TEXTURES IN WEAPONS FOLDER ===");
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Characters/Weapons" });
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"  - {System.IO.Path.GetFileName(path)}");
        }
    }
}
