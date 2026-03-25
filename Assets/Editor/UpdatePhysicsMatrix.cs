using UnityEditor;
using UnityEngine;

public class UpdatePhysicsMatrix
{
    public static void Execute()
    {
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int defaultLayer = LayerMask.NameToLayer("Default");
        
        if (projectileLayer != -1 && defaultLayer != -1)
        {
            Physics.IgnoreLayerCollision(projectileLayer, defaultLayer, true);
            
            // Also try to save it via SerializedObject
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset");
            if (assets != null && assets.Length > 0)
            {
                SerializedObject so = new SerializedObject(assets[0]);
                so.Update();
                
                // The collision matrix is stored as an array of uints, one for each layer.
                // But wait, m_LayerCollisionMatrix is a string in the YAML? No, it's a uint array in SerializedObject.
                // Actually, Physics.IgnoreLayerCollision in Editor mode DOES modify the asset, but we might need to save it.
                EditorUtility.SetDirty(assets[0]);
                AssetDatabase.SaveAssets();
                Debug.Log("Saved DynamicsManager.asset");
            }
        }
    }
}