using UnityEditor;
using UnityEngine;

public class FixPlayerModelGrounding
{
    public static void Execute()
    {
        // Fix in scene
        GameObject playerModel = GameObject.Find("PlayerModel");
        if (playerModel != null)
        {
            // Disable Apply Root Motion
            Animator animator = playerModel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                Debug.Log("Disabled Apply Root Motion on PlayerModel.");
            }

            // The model bounds show min Y = 0.11 world space, meaning feet are ~0.11 above bridge.
            // Player is at Y=0.25 world, Tier0_Base at Y=0 local, PlayerModel at Y=0 local.
            // The model's feet are at ~0.11 world Y. Bridge floor is at Y=0.
            // We need to shift the model down by ~0.11 / scale(0.1) = 1.1 units locally.
            playerModel.transform.localPosition = new Vector3(0, -1.1f, 0);
            Debug.Log("Adjusted PlayerModel local Y to -1.1 to ground feet on bridge.");
        }
        else
        {
            Debug.LogError("Could not find PlayerModel in scene.");
        }
    }
}