using UnityEngine;
using UnityEditor;
using System.IO;

public class SetupPlayer1Materials
{
    [MenuItem("Tools/Setup Player1 Materials")]
    public static void Execute()
    {
        string fbxPath = "Assets/Characters/Player1.fbx";
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null) { Debug.LogError("ModelImporter not found at " + fbxPath); return; }

        // Step 1: Set material creation mode
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.SaveAndReimport();
        Debug.Log("Set materialImportMode=ImportViaMaterialDescription, materialLocation=External");

        // Step 2: Extract textures
        string textureDest = "Assets/Characters/Textures";
        if (!Directory.Exists(textureDest))
            Directory.CreateDirectory(textureDest);

        // ExtractTextures moves embedded textures to the destination folder
        importer.ExtractTextures(textureDest);
        Debug.Log($"Extracted textures to {textureDest}");

        // Step 3: Extract materials
        string materialDest = "Assets/Characters/Materials";
        if (!Directory.Exists(materialDest))
            Directory.CreateDirectory(materialDest);

        // Extract each embedded material
        var embeddedMaterials = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        int matCount = 0;
        foreach (var asset in embeddedMaterials)
        {
            if (asset is Material mat)
            {
                string matPath = $"{materialDest}/{mat.name}.mat";
                // Only extract if not already there
                if (!File.Exists(matPath))
                {
                    Material extracted = new Material(mat);
                    AssetDatabase.CreateAsset(extracted, matPath);
                    matCount++;
                    Debug.Log($"Extracted material: {mat.name} -> {matPath}");
                }
                else
                {
                    Debug.Log($"Material already exists: {matPath}");
                }
            }
        }
        Debug.Log($"Extracted {matCount} material(s)");

        // Step 4: Remap materials on the importer to the extracted ones
        importer.SearchAndRemapMaterials(ModelImporterMaterialName.BasedOnMaterialName,
                                          ModelImporterMaterialSearch.Everywhere);

        importer.SaveAndReimport();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Done! Player1.fbx materials set up. Check PlayerModel in scene.");
    }
}
