using UnityEngine;
using UnityEditor;

public class FixProjectileLayerMatrix
{
    [MenuItem("Tools/Fix Projectile Layer Matrix")]
    public static void Execute()
    {
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int enemyLayer      = LayerMask.NameToLayer("Enemy");
        int playerLayer     = LayerMask.NameToLayer("Player");
        int groundLayer     = LayerMask.NameToLayer("Ground");

        Debug.Log($"Layers — Projectile:{projectileLayer} Enemy:{enemyLayer} Player:{playerLayer} Ground:{groundLayer}");

        // Log current state before changes
        for (int i = 0; i < 32; i++)
        {
            string name = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(name)) continue;
            bool ignored = Physics.GetIgnoreLayerCollision(projectileLayer, i);
            Debug.Log($"  Projectile vs {name}({i}): {(ignored ? "IGNORED" : "ENABLED")}");
        }

        // Ignore ALL layers for projectile first
        for (int i = 0; i < 32; i++)
            Physics.IgnoreLayerCollision(projectileLayer, i, true);

        // Then ONLY enable Projectile vs Enemy
        if (enemyLayer >= 0)
            Physics.IgnoreLayerCollision(projectileLayer, enemyLayer, false);

        // Persist via TagManager — write directly to DynamicsManager via SerializedObject
        var dynManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset");
        if (dynManager != null && dynManager.Length > 0)
        {
            SerializedObject so = new SerializedObject(dynManager[0]);
            so.Update();

            // Build the 32x32 collision matrix bitmask
            // Each layer i has a 32-bit mask of which layers it collides with
            SerializedProperty matrixProp = so.FindProperty("m_LayerCollisionMatrix");
            if (matrixProp != null)
            {
                // Read current matrix as array of 32 uints
                uint[] matrix = new uint[32];
                for (int i = 0; i < 32; i++)
                    matrix[i] = (uint)matrixProp.GetArrayElementAtIndex(i).longValue;

                // Clear all bits for projectile layer (both directions)
                matrix[projectileLayer] = 0;
                for (int i = 0; i < 32; i++)
                    matrix[i] &= ~(1u << projectileLayer);

                // Set only Enemy bit for projectile layer (both directions)
                if (enemyLayer >= 0)
                {
                    matrix[projectileLayer] |= (1u << enemyLayer);
                    matrix[enemyLayer] |= (1u << projectileLayer);
                }

                for (int i = 0; i < 32; i++)
                    matrixProp.GetArrayElementAtIndex(i).longValue = matrix[i];

                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log("DynamicsManager layer matrix saved.");
            }
            else
            {
                Debug.LogWarning("m_LayerCollisionMatrix property not found — matrix saved via Physics.IgnoreLayerCollision only (runtime).");
            }
        }

        Debug.Log("Done — Projectile layer now only collides with Enemy layer.");
    }
}
