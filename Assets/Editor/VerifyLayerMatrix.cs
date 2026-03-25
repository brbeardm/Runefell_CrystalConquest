using UnityEngine;
using UnityEditor;

public class VerifyLayerMatrix
{
    [MenuItem("Tools/Verify Layer Matrix")]
    public static void Execute()
    {
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        Debug.Log("=== Projectile layer collision state ===");
        for (int i = 0; i < 32; i++)
        {
            string name = LayerMask.LayerToName(i);
            if (string.IsNullOrEmpty(name)) continue;
            bool ignored = Physics.GetIgnoreLayerCollision(projectileLayer, i);
            string status = ignored ? "IGNORED" : "COLLIDES";
            string flag = (!ignored && i != enemyLayer) ? " ← PROBLEM" : "";
            Debug.Log($"  Projectile vs {name}({i}): {status}{flag}");
        }
    }
}
