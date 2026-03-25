using UnityEngine;
using UnityEditor;

public class FixPlayerGrounding
{
    [MenuItem("Tools/Fix Player Grounding")]
    public static void Execute()
    {
        // Find PlayerModel
        var playerModelGO = GameObject.Find("PlayerModel");
        if (playerModelGO == null) { Debug.LogError("PlayerModel not found!"); return; }

        Animator anim = playerModelGO.GetComponent<Animator>();
        if (anim == null) { Debug.LogError("No Animator!"); return; }

        // Disable root motion
        anim.applyRootMotion = false;

        // Get the lowest toe bone world Y - this is the true "foot contact" point
        Transform leftToe  = anim.GetBoneTransform(HumanBodyBones.LeftToes);
        Transform rightToe = anim.GetBoneTransform(HumanBodyBones.RightToes);
        Transform leftFoot  = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);

        // Find the lowest foot/toe Y in world space
        float lowestY = float.MaxValue;
        if (leftToe  != null) lowestY = Mathf.Min(lowestY, leftToe.position.y);
        if (rightToe != null) lowestY = Mathf.Min(lowestY, rightToe.position.y);
        if (leftFoot  != null) lowestY = Mathf.Min(lowestY, leftFoot.position.y);
        if (rightFoot != null) lowestY = Mathf.Min(lowestY, rightFoot.position.y);

        // Bridge top surface Y
        float bridgeTopY = 0.25f;

        // How much do we need to shift the model in world Y?
        float worldShift = bridgeTopY - lowestY;

        // Convert to local space (divide by world scale Y)
        float scaleY = playerModelGO.transform.lossyScale.y;
        float localShift = worldShift / scaleY;

        Vector3 currentLocal = playerModelGO.transform.localPosition;
        Vector3 newLocal = new Vector3(currentLocal.x, currentLocal.y + localShift, currentLocal.z);

        Debug.Log($"Lowest foot/toe world Y: {lowestY:F4}");
        Debug.Log($"Bridge top Y: {bridgeTopY:F4}");
        Debug.Log($"World shift needed: {worldShift:F4}");
        Debug.Log($"Scale Y (lossy): {scaleY:F4}");
        Debug.Log($"Local shift: {localShift:F4}");
        Debug.Log($"Old local Y: {currentLocal.y:F4}  ->  New local Y: {newLocal.y:F4}");

        Undo.RecordObject(playerModelGO.transform, "Fix Player Grounding");
        playerModelGO.transform.localPosition = newLocal;

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"Done! PlayerModel local Y set to {newLocal.y:F4}");

        // Reset scene view to perspective, framing the player
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv != null)
        {
            sv.orthographic = false;
            sv.LookAt(playerModelGO.transform.position + Vector3.up * 0.9f,
                      Quaternion.Euler(15f, 180f, 0f), 3f);
            sv.Repaint();
        }
    }
}
