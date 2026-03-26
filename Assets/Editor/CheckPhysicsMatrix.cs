using UnityEngine;
using UnityEditor;

public static class CheckPhysicsMatrix
{
    public static void Execute()
    {
        // Print all named layers
        Debug.Log("=== Layers ===");
        for (int i = 0; i < 32; i++)
        {
            string name = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(name))
                Debug.Log($"  Layer {i}: {name}");
        }

        // Check collision between key layer pairs
        Debug.Log("=== Collision Matrix (false = IGNORED) ===");
        string[] layersToCheck = { "Default", "Player", "Enemy", "Projectile", "Ground" };
        foreach (var a in layersToCheck)
        {
            foreach (var b in layersToCheck)
            {
                int la = LayerMask.NameToLayer(a);
                int lb = LayerMask.NameToLayer(b);
                if (la < 0 || lb < 0) continue;
                bool ignored = Physics.GetIgnoreLayerCollision(la, lb);
                if (ignored)
                    Debug.Log($"  {a} <-> {b}: IGNORED");
            }
        }
        Debug.Log("=== Done ===");
    }
}
