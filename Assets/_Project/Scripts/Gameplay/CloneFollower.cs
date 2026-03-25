using UnityEngine;

/// <summary>
/// Makes a clone shooter follow the player shoulder-to-shoulder,
/// slightly behind on Z. Flat bridge.
/// </summary>
public class CloneFollower : MonoBehaviour
{
    [HideInInspector] public Transform target;
    [HideInInspector] public float xOffset;
    [HideInInspector] public float zOffset;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = target.position + new Vector3(xOffset, 0f, zOffset);
        pos.x = Mathf.Clamp(pos.x, BridgeZoneConstants.BridgeMinX, BridgeZoneConstants.BridgeMaxX);
        transform.position = pos;
    }
}
