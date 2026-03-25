using UnityEngine;
using UnityEditor;

public class SetupCrystalProjectile
{
    [MenuItem("Tools/Setup Crystal Projectile")]
    public static void Setup()
    {
        // Create Material
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        // Surface Type: Transparent
        mat.SetFloat("_Surface", 1.0f); // 1 = Transparent
        mat.SetFloat("_Blend", 0.0f); // 0 = Alpha
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Base color: ice-blue (hex #99DDFF, alpha ~0.7)
        Color baseColor;
        ColorUtility.TryParseHtmlString("#99DDFF", out baseColor);
        baseColor.a = 0.7f;
        mat.SetColor("_BaseColor", baseColor);

        // Metallic 0.8, Smoothness 1.0
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 1.0f);

        // Enable Emission: color #66CCFF at intensity 3
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        Color emissionColor;
        ColorUtility.TryParseHtmlString("#66CCFF", out emissionColor);
        // Intensity 3
        float intensity = 3.0f;
        mat.SetColor("_EmissionColor", emissionColor * intensity);

        AssetDatabase.CreateAsset(mat, "Assets/_Project/Prefabs/Shooters/CrystalShard_Mat.mat");
        AssetDatabase.SaveAssets();

        // Update Prefab
        string prefabPath = "Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            // Update Mesh
            MeshFilter mf = prefab.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Mesh crystalMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Prefabs/Shooters/CrystalShardMesh.asset");
                mf.sharedMesh = crystalMesh;
            }

            // Update Material
            MeshRenderer mr = prefab.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = mat;
            }

            // Update Scale
            prefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.2f);

            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("Prefab updated successfully!");
        }
        else
        {
            Debug.LogError("Prefab not found at " + prefabPath);
        }
    }
}
