using UnityEngine;
using UnityEditor;

public class LowSideView
{
    [MenuItem("Tools/Low Side View")]
    public static void Execute()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null && SceneView.sceneViews.Count > 0)
            sv = (SceneView)SceneView.sceneViews[0];
        if (sv == null) { Debug.LogError("No SceneView"); return; }

        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found"); return; }

        Vector3 playerPos = player.transform.position;

        // Low angle side view - slightly above ground level, looking from the side
        sv.orthographic = false;
        sv.pivot = new Vector3(playerPos.x, playerPos.y + 0.5f, playerPos.z);
        sv.rotation = Quaternion.Euler(5f, 90f, 0f); // nearly horizontal, from the right
        sv.size = 2.5f;
        sv.Repaint();

        Debug.Log($"Low side view. Player world pos: {playerPos}");
    }
}
