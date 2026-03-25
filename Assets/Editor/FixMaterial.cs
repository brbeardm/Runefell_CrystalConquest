using UnityEngine;
using UnityEditor;

public class FixMaterial
{
    [MenuItem("Tools/Fix Material")]
    public static void Fix()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Shooters/CrystalShard_Mat.mat");
        if (mat != null)
        {
            mat.SetFloat("_Surface", 1.0f); // Transparent
            mat.SetFloat("_Blend", 0.0f); // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHATEST_ON"); // Sometimes needed for transparent
            
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
            // Ensure emission is enabled
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("Material fixed!");
        }
    }
}
