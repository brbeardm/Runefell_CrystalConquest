using UnityEngine;
using UnityEditor;

public class DebugEnemyAvoidance
{
    [MenuItem("Tools/Debug Enemy Avoidance")]
    public static void Execute()
    {
        var crystal = Object.FindAnyObjectByType<HeroCrystal>();
        if (crystal == null) { Debug.LogError("No HeroCrystal in scene"); return; }

        Bounds b = crystal.CrystalBounds;
        Debug.Log($"=== HeroCrystal ===");
        Debug.Log($"  IsPresent: {crystal.IsPresent}");
        Debug.Log($"  Bounds center: {b.center}");
        Debug.Log($"  Bounds min: {b.min}  max: {b.max}");
        Debug.Log($"  Bounds size: {b.size}");
        Debug.Log($"  World position: {crystal.transform.position}");

        float margin = 0.7f;
        float lookahead = 5f;
        Debug.Log($"=== Avoidance Zone (margin={margin}, lookahead={lookahead}) ===");
        Debug.Log($"  safeXMin={b.min.x - margin:F2}  safeXMax={b.max.x + margin:F2}");
        Debug.Log($"  safeZMin={b.min.z - margin:F2}  safeZMax={b.max.z + margin:F2}");
        Debug.Log($"  Steer starts at Z={b.max.z + margin + lookahead:F2}");
        Debug.Log($"  Blocked (stop forward) at Z <= {b.max.z + margin:F2}");

        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Debug.Log($"=== Enemies ({enemies.Length}) ===");
        foreach (var e in enemies)
        {
            Vector3 p = e.transform.position;
            bool inX = p.x > b.min.x - margin && p.x < b.max.x + margin;
            bool inZ = p.z < b.max.z + margin + lookahead && p.z > b.min.z - margin;
            bool blocked = p.z <= b.max.z + margin;
            Debug.Log($"  Enemy pos={p}  inXDanger={inX}  inZDanger={inZ}  blocked={blocked}");
        }
    }
}
