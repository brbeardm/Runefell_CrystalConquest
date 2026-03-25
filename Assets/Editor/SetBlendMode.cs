using UnityEngine;
using UnityEditor;

public class SetBlendMode
{
    [MenuItem("Tools/Set Blend Mode")]
    public static void Set()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Shooters/CrystalShard_Mat.mat");
        if (mat != null)
        {
            mat.SetFloat("_SrcBlend", 5.0f);
            mat.SetFloat("_DstBlend", 10.0f);
            mat.SetFloat("_SrcBlendAlpha", 1.0f);
            mat.SetFloat("_DstBlendAlpha", 10.0f);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("Blend mode set!");
        }
    }
}
