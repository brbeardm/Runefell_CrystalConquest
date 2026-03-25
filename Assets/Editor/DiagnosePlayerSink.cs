using UnityEngine;
using UnityEditor;

public class DiagnosePlayerSink
{
    [MenuItem("Tools/Diagnose Player Sink")]
    public static void Execute()
    {
        // Find all colliders on the player hierarchy
        var playerParent = GameObject.Find("---Player---");
        if (playerParent == null) { Debug.LogError("---Player--- not found (must be in play mode)"); return; }

        Debug.Log("=== Player Hierarchy Colliders ===");
        var allColliders = playerParent.GetComponentsInChildren<Collider>(true);
        foreach (var col in allColliders)
        {
            Debug.Log($"  [{col.gameObject.name}] {col.GetType().Name} | isTrigger={col.isTrigger} | enabled={col.enabled} | layer={LayerMask.LayerToName(col.gameObject.layer)}");
        }

        // Find all rigidbodies
        Debug.Log("=== Player Hierarchy Rigidbodies ===");
        var allRbs = playerParent.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in allRbs)
        {
            Debug.Log($"  [{rb.gameObject.name}] isKinematic={rb.isKinematic} | useGravity={rb.useGravity} | velocity={rb.linearVelocity}");
        }

        // Find the PlayerMover and log its _playerY via reflection
        var mover = playerParent.GetComponentInChildren<PlayerMover>();
        if (mover != null)
        {
            var field = typeof(PlayerMover).GetField("_playerY",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                Debug.Log($"PlayerMover._playerY = {field.GetValue(mover)}");

            Debug.Log($"PlayerMover GameObject world Y = {mover.transform.position.y:F4}");
        }

        // Check bridge floor
        var bridge = GameObject.Find("BridgeFloor");
        if (bridge != null)
        {
            Debug.Log($"BridgeFloor world Y center = {bridge.transform.position.y:F4}, scale.y = {bridge.transform.lossyScale.y:F4}");
            Debug.Log($"BridgeFloor top surface Y = {bridge.transform.position.y + bridge.transform.lossyScale.y * 0.5f:F4}");
        }
    }
}
