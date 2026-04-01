using UnityEngine;
using UnityEditor;

public class FixTrollGrounding
{
    [MenuItem("Tools/Fix Troll Grounding")]
    public static void Execute()
    {
        GameObject troll = GameObject.Find("Troll_Idle_5meters");
        if (troll == null)
        {
            Debug.LogError("Troll_Idle_5meters not found!");
            return;
        }

        // Get the mesh renderer to find the bounds
        SkinnedMeshRenderer smr = troll.transform.Find("Mesh_0.001")?.GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError("SkinnedMeshRenderer not found!");
            return;
        }

        // Get the mesh bounds relative to the root
        Bounds meshBounds = smr.bounds;
        float meshMinY = meshBounds.min.y;
        float meshMaxY = meshBounds.max.y;
        float meshHeight = meshMaxY - meshMinY;
        
        Debug.Log($"Mesh bounds: Min Y = {meshMinY}, Max Y = {meshMaxY}, Height = {meshHeight}");
        Debug.Log($"Mesh center Y = {meshBounds.center.y}");

        // The feet should be at the bridge surface (Y = 0.25)
        // Currently the mesh min is at -0.50 relative to root
        // We need to raise the root so that mesh min aligns with bridge surface
        
        float bridgeSurfaceY = 0.25f;
        float requiredRootY = bridgeSurfaceY - meshMinY;
        
        Debug.Log($"Bridge surface Y: {bridgeSurfaceY}");
        Debug.Log($"Current root Y: {troll.transform.position.y}");
        Debug.Log($"Required root Y: {requiredRootY}");
        
        // Update position
        Vector3 newPos = troll.transform.position;
        newPos.y = requiredRootY;
        troll.transform.position = newPos;
        
        EditorUtility.SetDirty(troll.transform);
        Debug.Log($"Troll grounding fixed! New position: {troll.transform.position}");
    }
}
