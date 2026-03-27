using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AddVictoryPanel
{
    [MenuItem("Tools/Add Victory Panel")]
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

        // Remove existing VictoryPanel if present
        var existing = canvas.transform.Find("VictoryPanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // ── VictoryPanel ───────────────────────────────────────────────────
        var panel = new GameObject("VictoryPanel");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<CanvasRenderer>();
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.82f);
        panelImg.raycastTarget = true;

        var panelRT = panel.GetComponent<RectTransform>();
        // Stretch full screen
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        panelRT.pivot = new Vector2(0.5f, 0.5f);

        panel.SetActive(false);

        // ── VictoryTitleText ───────────────────────────────────────────────
        var titleGO = new GameObject("VictoryTitleText");
        titleGO.transform.SetParent(panel.transform, false);
        titleGO.AddComponent<CanvasRenderer>();
        var titleText = titleGO.AddComponent<Text>();
        titleText.text = "YOU ARE THE CRYSTAL KING!";
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(1f, 0.92f, 0f, 1f); // bold yellow
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.resizeTextForBestFit = false;

        var titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax = new Vector2(0.5f, 0.5f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.sizeDelta = new Vector2(800f, 100f);
        titleRT.anchoredPosition = new Vector2(0f, 100f);

        // ── VictoryScoreText ───────────────────────────────────────────────
        var scoreGO = new GameObject("VictoryScoreText");
        scoreGO.transform.SetParent(panel.transform, false);
        scoreGO.AddComponent<CanvasRenderer>();
        var scoreText = scoreGO.AddComponent<Text>();
        scoreText.text = "Final Score: 0";
        scoreText.fontSize = 24;
        scoreText.fontStyle = FontStyle.Normal;
        scoreText.color = Color.white;
        scoreText.alignment = TextAnchor.MiddleCenter;

        var scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRT.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRT.pivot = new Vector2(0.5f, 0.5f);
        scoreRT.sizeDelta = new Vector2(600f, 80f);
        scoreRT.anchoredPosition = new Vector2(0f, -20f);

        // ── VictoryRestartButton ───────────────────────────────────────────
        var btnGO = new GameObject("VictoryRestartButton");
        btnGO.transform.SetParent(panel.transform, false);
        btnGO.AddComponent<CanvasRenderer>();
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 0.2f, 1f); // green button
        btnImg.raycastTarget = true;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.sizeDelta = new Vector2(400f, 120f);
        btnRT.anchoredPosition = new Vector2(0f, -160f);

        // Button label
        var btnLabelGO = new GameObject("Text");
        btnLabelGO.transform.SetParent(btnGO.transform, false);
        btnLabelGO.AddComponent<CanvasRenderer>();
        var btnLabel = btnLabelGO.AddComponent<Text>();
        btnLabel.text = "Play Again";
        btnLabel.fontSize = 28;
        btnLabel.fontStyle = FontStyle.Bold;
        btnLabel.color = Color.white;
        btnLabel.alignment = TextAnchor.MiddleCenter;

        var btnLabelRT = btnLabelGO.GetComponent<RectTransform>();
        btnLabelRT.anchorMin = Vector2.zero;
        btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero;
        btnLabelRT.offsetMax = Vector2.zero;
        btnLabelRT.pivot = new Vector2(0.5f, 0.5f);

        // ── Assign to UIManager ────────────────────────────────────────────
        var uiManagers = canvas.GetComponents<UIManager>();
        foreach (var uiManager in uiManagers)
        {
            var so = new SerializedObject(uiManager);
            so.FindProperty("victoryPanel").objectReferenceValue = panel;
            so.FindProperty("victoryTitleText").objectReferenceValue = titleText;
            so.FindProperty("victoryScoreText").objectReferenceValue = scoreText;
            so.FindProperty("victoryRestartButton").objectReferenceValue = btn;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("VictoryPanel created and assigned to UIManager successfully.");
    }
}
