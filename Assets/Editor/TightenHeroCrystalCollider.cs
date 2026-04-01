using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class TightenHeroCrystalCollider
{
    public static void Execute()
    {
        Debug.Log("=== TIGHTENING HERO CRYSTAL COLLIDER ===");

        // Find HeroCrystal in the scene
        var heroCrystal = GameObject.Find("HeroCrystal");
        if (heroCrystal == null)
        {
            Debug.LogError("Could not find HeroCrystal in scene!");
            return;
        }

        // Get the BoxCollider
        var collider = heroCrystal.GetComponent<BoxCollider>();
        if (collider == null)
        {
            Debug.LogError("HeroCrystal has no BoxCollider!");
            return;
        }

        Debug.Log($"[Before] Collider Size: {collider.size}");
        Debug.Log($"[Before] Collider Center: {collider.center}");

        // Tighten the collider to fit the crystal pieces
        // Crystal pieces span from approximately:
        // X: -3.675 to -1.525 (width ~2.15, but scaled by 0.8 = 1.72 local)
        // Y: 0.47 to 3.5 (height ~3.03)
        // Z: -4.075 to -1.88 (depth ~2.195, but scaled by 0.8 = 1.756 local)

        collider.size = new Vector3(1.5f, 3.0f, 1.8f);
        collider.center = new Vector3(0.0f, 1.5f, 0.0f);

        Debug.Log($"[After] Collider Size: {collider.size}");
        Debug.Log($"[After] Collider Center: {collider.center}");

        // Mark the scene as dirty so changes are saved
        EditorSceneManager.MarkSceneDirty(heroCrystal.scene);

        Debug.Log("=== HERO CRYSTAL COLLIDER TIGHTENED ===");
    }
}
