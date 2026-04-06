using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class WorldPositionDisplay
{
    static WorldPositionDisplay()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeTransform == null)
            return;

        Transform t = Selection.activeTransform;
        Vector3 worldPos = t.position;

        Handles.BeginGUI();
        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.richText = true;

        string label = $"<color=yellow>{t.name}</color>\n" +
                       $"World X: {worldPos.x:F4}\n" +
                       $"World Y: {worldPos.y:F4}\n" +
                       $"World Z: {worldPos.z:F4}";

        GUI.Label(new Rect(10, 10, 300, 90), label, style);
        Handles.EndGUI();
    }
}
