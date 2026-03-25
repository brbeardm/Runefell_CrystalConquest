using UnityEngine;
using UnityEditor;

public class SideViewPlayer
{
    [MenuItem("Tools/Side View Player")]
    public static void Execute()
    {
        SceneView sv = SceneView.lastActiveSceneView;
        if (sv == null) sv = SceneView.sceneViews.Count > 0 ? (SceneView)SceneView.sceneViews[0] : null;
        if (sv == null) { Debug.LogError("No SceneView found"); return; }

        // Find player
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found"); return; }

        Vector3 playerPos = player.transform.position;

        // Set orthographic side view (looking from the right, along -X axis)
        sv.orthographic = true;
        sv.size = 3f;
        sv.pivot = new Vector3(playerPos.x, playerPos.y + 0.9f, playerPos.z);
        sv.rotation = Quaternion.Euler(0f, 90f, 0f); // look from right side
        sv.Repaint();

        Debug.Log($"Side view set. Player world pos: {playerPos}");
    }
}
