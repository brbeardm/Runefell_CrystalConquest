using UnityEngine;
using UnityEditor;

public class CompareTrollMaterials
{
    [MenuItem("Tools/Compare Troll Materials")]
    public static void Execute()
    {
        // Find both trolls
        GameObject trollIdle = GameObject.Find("Troll_Idle_5meters");
        GameObject trollBoss = GameObject.Find("Enemy_Boss_Troll");

        if (trollIdle == null || trollBoss == null)
        {
            Debug.LogError("Could not find both trolls!");
            return;
        }

        // Get mesh renderers
        SkinnedMeshRenderer idleRenderer = trollIdle.transform.Find("Mesh_0.001").GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer bossRenderer = trollBoss.transform.Find("Mesh_0.002").GetComponent<SkinnedMeshRenderer>();

        if (idleRenderer == null || bossRenderer == null)
        {
            Debug.LogError("Could not find renderers!");
            return;
        }

        Debug.Log("=== TROLL_IDLE_5METERS MATERIALS ===");
        Material[] idleMaterials = idleRenderer.materials;
        Debug.Log($"Total materials: {idleMaterials.Length}");
        for (int i = 0; i < idleMaterials.Length; i++)
        {
            Debug.Log($"  [{i}] {idleMaterials[i].name} - Shader: {idleMaterials[i].shader.name}");
        }

        Debug.Log("\n=== ENEMY_BOSS_TROLL MATERIALS ===");
        Material[] bossMaterials = bossRenderer.materials;
        Debug.Log($"Total materials: {bossMaterials.Length}");
        for (int i = 0; i < bossMaterials.Length; i++)
        {
            Debug.Log($"  [{i}] {bossMaterials[i].name} - Shader: {bossMaterials[i].shader.name}");
        }

        Debug.Log("\n=== COPYING BOSS MATERIALS TO IDLE ===");
        idleRenderer.materials = bossMaterials;
        EditorUtility.SetDirty(idleRenderer);
        Debug.Log("Materials copied successfully!");
    }
}
