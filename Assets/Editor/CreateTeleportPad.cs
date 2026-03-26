using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateTeleportPad
{
    [MenuItem("Tools/Create Teleport Pad")]
    public static void Execute()
    {
        // Find HeroCrystal
        GameObject heroCrystal = GameObject.Find("HeroCrystal");
        if (heroCrystal == null)
        {
            Debug.LogError("HeroCrystal not found in scene.");
            return;
        }

        // Create Cylinder primitive
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pad.name = "TeleportPad";
        pad.transform.SetParent(heroCrystal.transform, false);

        // Set local position and scale
        pad.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        pad.transform.localScale = new Vector3(2f, 0.05f, 2f);

        // Remove CapsuleCollider (visual only)
        CapsuleCollider col = pad.GetComponent<CapsuleCollider>();
        if (col != null)
        {
            Object.DestroyImmediate(col);
            Debug.Log("Removed CapsuleCollider from TeleportPad.");
        }

        // Add TeleportPad component
        pad.AddComponent<TeleportPad>();
        Debug.Log("Added TeleportPad component.");

        // Create material with URP/Lit shader, emission, green base color
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", Color.green);

        // Enable emission
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        mat.SetColor("_EmissionColor", new Color(0f, 0.5f, 0f, 1f));

        // Save material as asset
        string matPath = "Assets/_Project/Prefabs/TeleportPad_Mat.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created material at {matPath}");

        // Assign material to renderer
        MeshRenderer renderer = pad.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = mat;
            Debug.Log("Assigned material to TeleportPad renderer.");
        }

        // Mark dirty and save
        EditorUtility.SetDirty(pad);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("TeleportPad created and scene saved.");
    }
}
