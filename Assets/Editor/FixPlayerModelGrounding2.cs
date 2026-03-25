using UnityEditor;
using UnityEngine;

public class FixPlayerModelGrounding2
{
    public static void Execute()
    {
        GameObject playerModel = GameObject.Find("PlayerModel");
        if (playerModel == null)
        {
            Debug.LogError("Could not find PlayerModel.");
            return;
        }

        // Player world Y = 0.25
        // We want feet (bounds min Y) to be at world Y = 0
        // bounds min Y world = playerWorldY + playerModelLocalY + boundsMinLocal
        // At localY = -1.1: bounds min world = 0.25 + (-1.1*1) + (-0.988 - (-1.1)) = 0.25 + (-1.1) + 0.112 = -0.738
        // Wait - bounds are already in world space from the API.
        // At localY = -1.1: bounds min world = -0.988
        // We need bounds min world = 0 (bridge surface)
        // Difference = 0 - (-0.988) = +0.988 in world space
        // Since scale is 1 at the Player level, localY adjustment = +0.988
        // New localY = -1.1 + 0.988 = -0.112
        
        playerModel.transform.localPosition = new Vector3(0, -0.112f, 0);
        Debug.Log($"Set PlayerModel localY to -0.112. Bounds min Y should now be at ~0 (bridge surface).");
    }
}