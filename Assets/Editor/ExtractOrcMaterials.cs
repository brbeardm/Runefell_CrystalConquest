using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ExtractOrcMaterials
{
    public static void Execute()
    {
        string fbxPath = "Assets/Characters/enemy_orc.fbx";
        string texDir = "Assets/Characters/Textures";
        string matDir = "Assets/Characters/Materials";
        
        if (!AssetDatabase.IsValidFolder(texDir))
        {
            AssetDatabase.CreateFolder("Assets/Characters", "Textures");
        }
        if (!AssetDatabase.IsValidFolder(matDir))
        {
            AssetDatabase.CreateFolder("Assets/Characters", "Materials");
        }
        
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            // Extract Textures
            bool texExtracted = importer.ExtractTextures(texDir);
            Debug.Log($"Extracted textures: {texExtracted}");
            
            // Extract Materials
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            List<Material> extractedMats = new List<Material>();
            
            foreach (Object asset in assets)
            {
                if (asset is Material mat && !mat.name.StartsWith("__preview__"))
                {
                    string newPath = $"{matDir}/{mat.name}.mat";
                    string uniquePath = AssetDatabase.GenerateUniqueAssetPath(newPath);
                    
                    string error = AssetDatabase.ExtractAsset(asset, uniquePath);
                    if (string.IsNullOrEmpty(error))
                    {
                        Debug.Log($"Extracted material: {uniquePath}");
                        extractedMats.Add(AssetDatabase.LoadAssetAtPath<Material>(uniquePath));
                    }
                    else
                    {
                        Debug.LogError($"Failed to extract material {mat.name}: {error}");
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Fix Materials
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                Debug.LogError("URP Lit shader not found!");
                return;
            }
            
            // Find the extracted texture
            string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { texDir });
            Texture2D orcTex = null;
            if (texGuids.Length > 0)
            {
                orcTex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(texGuids[0]));
                Debug.Log($"Found texture: {orcTex.name}");
            }
            
            foreach (Material mat in extractedMats)
            {
                mat.shader = urpLit;
                if (orcTex != null)
                {
                    mat.SetTexture("_BaseMap", orcTex);
                }
                EditorUtility.SetDirty(mat);
                Debug.Log($"Fixed material: {mat.name}");
            }
            
            AssetDatabase.SaveAssets();
            
            // Remap materials in importer
            importer.materialLocation = ModelImporterMaterialLocation.External;
            importer.materialName = ModelImporterMaterialName.BasedOnMaterialName;
            importer.materialSearch = ModelImporterMaterialSearch.Local;
            
            foreach (Material mat in extractedMats)
            {
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), mat.name), mat);
            }
            
            importer.SaveAndReimport();
            Debug.Log("Remapped materials in FBX.");
        }
        else
        {
            Debug.LogError("Could not find enemy_orc.fbx");
        }
    }
}