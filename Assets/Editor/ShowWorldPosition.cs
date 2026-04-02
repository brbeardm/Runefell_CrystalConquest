using UnityEngine;
using UnityEditor;

/// <summary>
/// Shows world position of the selected GameObject in a floating window.
/// Open via Window > Show World Position
/// </summary>
public class ShowWorldPosition : EditorWindow
{
    [MenuItem("Window/Show World Position")]
    public static void Open()
    {
        GetWindow<ShowWorldPosition>("World Position");
    }

    private void OnGUI()
    {
        if (Selection.activeTransform == null)
        {
            EditorGUILayout.LabelField("Select a GameObject to see its world position.");
            return;
        }

        var t = Selection.activeTransform;
        EditorGUILayout.LabelField("Object:", t.name, EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.Vector3Field("World Position", t.position);
        EditorGUILayout.Vector3Field("World Rotation", t.eulerAngles);
        EditorGUILayout.Vector3Field("Lossy Scale", t.lossyScale);
    }

    private void OnSelectionChange() => Repaint();
    private void Update() => Repaint();
}
