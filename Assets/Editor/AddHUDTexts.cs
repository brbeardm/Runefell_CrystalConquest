using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AddHUDTexts
{
    [MenuItem("Tools/Add HUD Texts")]
    public static void Execute()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // Find Canvas
        GameObject canvas = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Canvas") { canvas = root; break; }
        }
        if (canvas == null) { Debug.LogError("Canvas not found!"); return; }

        // ── 1. WaveCounterText ─────────────────────────────────────────────
        // Anchored top-left, below WaveText (WaveText is at anchoredPos (0,-50), height 100)
        // Place at top-left similar to ScoreText but one row lower
        GameObject waveCounterGO = CreateLegacyText(canvas, "WaveCounterText");
        var waveCounterRT = waveCounterGO.GetComponent<RectTransform>();
        // Top-left anchor
        waveCounterRT.anchorMin = new Vector2(0f, 1f);
        waveCounterRT.anchorMax = new Vector2(0f, 1f);
        waveCounterRT.pivot = new Vector2(0f, 1f);
        waveCounterRT.sizeDelta = new Vector2(400f, 100f);
        // Below ScoreText: ScoreText is at (50, -50), height 100 → place at (50, -160)
        waveCounterRT.anchoredPosition = new Vector2(50f, -160f);
        var waveCounterText = waveCounterGO.GetComponent<Text>();
        waveCounterText.text = "";
        waveCounterText.fontSize = 24;
        waveCounterText.color = Color.white;
        waveCounterText.alignment = TextAnchor.UpperLeft;
        waveCounterGO.SetActive(true);

        // ── 2. BuffTimerText ───────────────────────────────────────────────
        // Anchored top-center, disabled by default
        GameObject buffTimerGO = CreateLegacyText(canvas, "BuffTimerText");
        var buffTimerRT = buffTimerGO.GetComponent<RectTransform>();
        buffTimerRT.anchorMin = new Vector2(0.5f, 1f);
        buffTimerRT.anchorMax = new Vector2(0.5f, 1f);
        buffTimerRT.pivot = new Vector2(0.5f, 1f);
        buffTimerRT.sizeDelta = new Vector2(400f, 80f);
        buffTimerRT.anchoredPosition = new Vector2(0f, -160f);
        var buffTimerText = buffTimerGO.GetComponent<Text>();
        buffTimerText.text = "BUFF 0.0s";
        buffTimerText.fontSize = 26;
        buffTimerText.color = new Color(1f, 0.85f, 0.1f, 1f); // golden yellow
        buffTimerText.alignment = TextAnchor.UpperCenter;
        buffTimerGO.SetActive(false);

        // ── 3. BreatherCountdownText ───────────────────────────────────────
        // Anchored center screen, font size 28, disabled by default
        GameObject breatherGO = CreateLegacyText(canvas, "BreatherCountdownText");
        var breatherRT = breatherGO.GetComponent<RectTransform>();
        breatherRT.anchorMin = new Vector2(0.5f, 0.5f);
        breatherRT.anchorMax = new Vector2(0.5f, 0.5f);
        breatherRT.pivot = new Vector2(0.5f, 0.5f);
        breatherRT.sizeDelta = new Vector2(600f, 80f);
        breatherRT.anchoredPosition = new Vector2(0f, 0f);
        var breatherText = breatherGO.GetComponent<Text>();
        breatherText.text = "Next wave in 5...";
        breatherText.fontSize = 28;
        breatherText.color = Color.white;
        breatherText.alignment = TextAnchor.MiddleCenter;
        breatherGO.SetActive(false);

        // ── Assign to UIManager ────────────────────────────────────────────
        // There are two UIManager components on Canvas; assign to both
        var uiManagers = canvas.GetComponents<UIManager>();
        foreach (var uiManager in uiManagers)
        {
            var so = new SerializedObject(uiManager);
            so.FindProperty("waveCounterText").objectReferenceValue = waveCounterText;
            so.FindProperty("buffTimerText").objectReferenceValue = buffTimerText;
            so.FindProperty("breatherCountdownText").objectReferenceValue = breatherText;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("HUD texts created and assigned to UIManager successfully.");
    }

    private static GameObject CreateLegacyText(GameObject parent, string name)
    {
        // Check if already exists and remove
        var existing = parent.transform.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Text>();
        return go;
    }
}
