using UnityEngine;
using UnityEditor;

public class SetupProjectileFX
{
    [MenuItem("Tools/Setup Projectile FX")]
    public static void Setup()
    {
        // 1. Create Trail Material
        Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        
        // Set to Additive
        trailMat.SetFloat("_Surface", 1.0f); // Transparent
        trailMat.SetFloat("_Blend", 2.0f); // Additive
        trailMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        trailMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        trailMat.SetInt("_ZWrite", 0);
        trailMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        trailMat.EnableKeyword("_BLENDMODE_ADD");
        trailMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Ice-blue color
        Color iceBlue;
        ColorUtility.TryParseHtmlString("#99DDFF", out iceBlue);
        trailMat.SetColor("_BaseColor", iceBlue);

        AssetDatabase.CreateAsset(trailMat, "Assets/_Project/Prefabs/Shooters/CrystalTrail_Mat.mat");
        AssetDatabase.SaveAssets();

        // 2. Update Prefab
        string prefabPath = "Assets/_Project/Scripts/Gameplay/CrystalProjectile.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            // Add TrailRenderer
            TrailRenderer tr = prefab.GetComponent<TrailRenderer>();
            if (tr == null)
            {
                tr = prefab.AddComponent<TrailRenderer>();
            }
            
            tr.time = 0.1f;
            tr.startWidth = 0.03f;
            tr.endWidth = 0.0f;
            tr.sharedMaterial = trailMat;
            tr.minVertexDistance = 0.01f; // Good for short trails
            tr.emitting = true;

            // Add ProjectileFX
            ProjectileFX pfx = prefab.GetComponent<ProjectileFX>();
            if (pfx == null)
            {
                pfx = prefab.AddComponent<ProjectileFX>();
            }
            
            // Assign trail to ProjectileFX
            SerializedObject so = new SerializedObject(pfx);
            so.Update();
            so.FindProperty("trail").objectReferenceValue = tr;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("Projectile FX setup complete!");
        }
        else
        {
            Debug.LogError("Prefab not found at " + prefabPath);
        }
    }
}
