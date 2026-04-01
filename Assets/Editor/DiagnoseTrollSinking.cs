using UnityEngine;
using UnityEditor;

public class DiagnoseTrollSinking
{
    [MenuItem("Tools/Diagnose Troll Sinking")]
    public static void Execute()
    {
        GameObject troll = GameObject.Find("Troll_Idle_5meters");
        if (troll == null)
        {
            Debug.LogError("Troll_Idle_5meters not found!");
            return;
        }

        Debug.Log("=== TROLL COMPONENTS ===");
        Debug.Log($"Position: {troll.transform.position}");
        Debug.Log($"Local Position: {troll.transform.localPosition}");
        
        Renderer renderer = troll.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Debug.Log($"Bounds Min: {renderer.bounds.min}");
            Debug.Log($"Bounds Max: {renderer.bounds.max}");
        }

        Rigidbody rb = troll.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Rigidbody found:");
            Debug.Log($"  - Mass: {rb.mass}");
            Debug.Log($"  - Use Gravity: {rb.useGravity}");
            Debug.Log($"  - Is Kinematic: {rb.isKinematic}");
            Debug.Log($"  - Velocity: {rb.linearVelocity}");
        }
        else
        {
            Debug.LogWarning("No Rigidbody found on Troll!");
        }

        Collider[] colliders = troll.GetComponents<Collider>();
        Debug.Log($"Colliders found: {colliders.Length}");
        foreach (Collider col in colliders)
        {
            Debug.Log($"  - {col.GetType().Name}: isTrigger={col.isTrigger}, center={col.bounds.center}");
        }

        // Check mesh collider
        SkinnedMeshRenderer smr = troll.transform.Find("Mesh_0.001")?.GetComponent<SkinnedMeshRenderer>();
        if (smr != null)
        {
            Debug.Log($"SkinnedMeshRenderer bounds: {smr.bounds}");
            Debug.Log($"  - Center: {smr.bounds.center}");
            Debug.Log($"  - Extents: {smr.bounds.extents}");
            Debug.Log($"  - Min: {smr.bounds.min}");
            Debug.Log($"  - Max: {smr.bounds.max}");
        }

        // Check BridgeFloor
        GameObject bridgeFloor = GameObject.Find("BridgeFloor");
        if (bridgeFloor != null)
        {
            Debug.Log("\n=== BRIDGE FLOOR ===");
            Debug.Log($"Position: {bridgeFloor.transform.position}");
            Debug.Log($"Bounds: {bridgeFloor.GetComponent<Renderer>().bounds}");
            
            BoxCollider bc = bridgeFloor.GetComponent<BoxCollider>();
            if (bc != null)
            {
                Debug.Log($"BoxCollider center: {bc.center}");
                Debug.Log($"BoxCollider size: {bc.size}");
                Debug.Log($"BoxCollider bounds: {bc.bounds}");
            }
        }
    }
}
