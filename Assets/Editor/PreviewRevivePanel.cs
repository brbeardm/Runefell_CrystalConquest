using UnityEditor;
using UnityEngine;

public class PreviewRevivePanel
{
    [MenuItem("Tools/Preview Revive Panel (Toggle)")]
    public static void Toggle()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null) { Debug.LogError("Canvas not found!"); return; }

        var panel = canvas.transform.Find("RevivePanel");
        if (panel == null) { Debug.LogError("RevivePanel not found on Canvas! Run Tools > Add Revive Panel first."); return; }

        bool newState = !panel.gameObject.activeSelf;
        panel.gameObject.SetActive(newState);
        Debug.Log($"RevivePanel {(newState ? "ON" : "OFF")}");
    }
}
