using UnityEngine;
using UnityEditor;

public class SaveLayerMatrix
{
    [MenuItem("Tools/Save Layer Matrix")]
    public static void Execute()
    {
        int projectileLayer = LayerMask.NameToLayer("Projectile"); // 8
        int enemyLayer      = LayerMask.NameToLayer("Enemy");      // 10

        // Ignore ALL layers for projectile
        for (int i = 0; i < 32; i++)
            Physics.IgnoreLayerCollision(projectileLayer, i, true);

        // Only enable Projectile <-> Enemy
        Physics.IgnoreLayerCollision(projectileLayer, enemyLayer, false);

        // Persist by writing directly to DynamicsManager.asset
        // Unity stores the matrix as a hex string in m_LayerCollisionMatrix
        // We rebuild it: for each layer i, bit j set = layer i collides with layer j
        uint[] matrix = new uint[32];

        // Start with all-collide (0xFFFFFFFF) then clear projectile row/col
        for (int i = 0; i < 32; i++)
            matrix[i] = 0xFFFFFFFF;

        // Clear all bits for projectile layer in every row
        for (int i = 0; i < 32; i++)
            matrix[i] &= ~(1u << projectileLayer);

        // Clear projectile row entirely
        matrix[projectileLayer] = 0;

        // Set only Enemy bit in projectile row (both directions)
        matrix[projectileLayer] |= (1u << enemyLayer);
        matrix[enemyLayer]      |= (1u << projectileLayer);

        // Convert to hex string (Unity stores it as a flat hex, low layer first)
        // Each uint is 4 bytes, 32 uints = 128 bytes = 256 hex chars
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < 32; i++)
        {
            // Little-endian byte order per uint
            uint v = matrix[i];
            sb.AppendFormat("{0:x2}{1:x2}{2:x2}{3:x2}",
                v & 0xFF, (v >> 8) & 0xFF, (v >> 16) & 0xFF, (v >> 24) & 0xFF);
        }

        string hexMatrix = sb.ToString();
        Debug.Log($"New matrix hex ({hexMatrix.Length} chars): {hexMatrix.Substring(0, 32)}...");

        // Read, modify, write DynamicsManager.asset
        string path = "ProjectSettings/DynamicsManager.asset";
        string[] lines = System.IO.File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("m_LayerCollisionMatrix:"))
            {
                lines[i] = "  m_LayerCollisionMatrix: " + hexMatrix;
                Debug.Log($"Updated m_LayerCollisionMatrix at line {i}");
                break;
            }
        }
        System.IO.File.WriteAllLines(path, lines);
        Debug.Log("DynamicsManager.asset written. Projectile now only collides with Enemy.");
    }
}
