using UnityEngine;
using UnityEditor;

public class MeasurePlayerFeet
{
    [MenuItem("Tools/Measure Player Feet")]
    public static void Execute()
    {
        // Find the Player
        var playerModelGO = GameObject.Find("PlayerModel");
        if (playerModelGO == null)
        {
            Debug.LogError("PlayerModel not found!");
            return;
        }

        // Get the Animator to find foot bones
        Animator anim = playerModelGO.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("No Animator on PlayerModel!");
            return;
        }

        // Get foot bone world positions
        Transform leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);
        Transform leftToe = anim.GetBoneTransform(HumanBodyBones.LeftToes);
        Transform rightToe = anim.GetBoneTransform(HumanBodyBones.RightToes);

        Debug.Log($"=== Player Feet Measurement ===");
        Debug.Log($"PlayerModel world position: {playerModelGO.transform.position}");
        Debug.Log($"PlayerModel local position: {playerModelGO.transform.localPosition}");
        Debug.Log($"PlayerModel scale: {playerModelGO.transform.localScale}");

        if (leftFoot != null) Debug.Log($"Left Foot world Y: {leftFoot.position.y:F4}");
        if (rightFoot != null) Debug.Log($"Right Foot world Y: {rightFoot.position.y:F4}");
        if (leftToe != null) Debug.Log($"Left Toe world Y: {leftToe.position.y:F4}");
        if (rightToe != null) Debug.Log($"Right Toe world Y: {rightToe.position.y:F4}");

        // Get the renderer bounds (actual mesh extents)
        Renderer[] renderers = playerModelGO.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds combined = renderers[0].bounds;
            foreach (var r in renderers)
                combined.Encapsulate(r.bounds);
            Debug.Log($"Combined mesh bounds min Y (world): {combined.min.y:F4}");
            Debug.Log($"Combined mesh bounds max Y (world): {combined.max.y:F4}");
        }

        // Bridge surface
        Debug.Log($"Bridge surface top Y: 0.25 (BridgeFloor center=0, scale.y=0.5, so top=0.25)");
        Debug.Log($"Player root world Y: {playerModelGO.transform.parent?.parent?.parent?.position.y:F4}");

        // Calculate needed offset
        if (renderers.Length > 0)
        {
            Bounds combined = renderers[0].bounds;
            foreach (var r in renderers)
                combined.Encapsulate(r.bounds);

            float feetWorldY = combined.min.y;
            float bridgeTopY = 0.25f;
            float worldDelta = bridgeTopY - feetWorldY;
            float localDelta = worldDelta / playerModelGO.transform.localScale.y;
            float newLocalY = playerModelGO.transform.localPosition.y + localDelta;
            Debug.Log($"Feet are at world Y={feetWorldY:F4}, bridge top at Y={bridgeTopY:F4}");
            Debug.Log($"Need to move world Y by: {worldDelta:F4}");
            Debug.Log($"Local Y delta needed: {localDelta:F4}");
            Debug.Log($"Suggested new local Y: {newLocalY:F4}");
        }
    }
}
