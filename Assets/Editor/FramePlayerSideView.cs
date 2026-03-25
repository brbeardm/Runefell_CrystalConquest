using UnityEngine;
using UnityEditor;

public class FramePlayerSideView
{
    [MenuItem("Tools/Frame Player Side View")]
    public static void Execute()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null && SceneView.sceneViews.Count > 0)
            sv = (SceneView)SceneView.sceneViews[0];
        if (sv == null) { Debug.LogError("No SceneView"); return; }

        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found"); return; }

        Vector3 playerPos = player.transform.position;

        // True side view from the right, slightly elevated, perspective
        sv.orthographic = false;
        sv.in2DMode = false;
        // Look from the right side (+X direction), slightly above
        // pivot at player center height (~0.9 units up from feet)
        sv.pivot = new Vector3(playerPos.x, playerPos.y + 0.9f, playerPos.z);
        sv.rotation = Quaternion.Euler(8f, -90f, 0f); // from +X side, slight downward tilt
        sv.size = 2.0f;
        sv.Repaint();

        Debug.Log($"Framed player from side. Player world pos: {playerPos}");
    }
}
