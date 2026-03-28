using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AddRevivePanel
{
    // Gold color used for borders and dividers
    private static readonly Color Gold = new Color(0.83f, 0.68f, 0.21f, 1f);
    private static readonly Color GoldDark = new Color(0.65f, 0.50f, 0.15f, 1f);
    private static readonly Color RoyalBlue = new Color(0.15f, 0.22f, 0.55f, 1f);

    [MenuItem("Tools/Add Revive Panel")]
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

        // Remove existing RevivePanel if present
        var existing = canvas.transform.Find("RevivePanel");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        // ── RevivePanel (full-screen dark overlay) ───────────────────────
        var panel = new GameObject("RevivePanel");
        panel.transform.SetParent(canvas.transform, false);
        panel.AddComponent<CanvasRenderer>();
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.02f, 0.02f, 0.05f, 0.90f);
        panelImg.raycastTarget = true;

        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        panel.SetActive(false);

        // ── Content container (centered vertical layout) ─────────────────
        var content = new GameObject("Content");
        content.transform.SetParent(panel.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(700f, 900f);
        contentRT.anchoredPosition = Vector2.zero;

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 18f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(40, 40, 0, 0);

        // ── Giant Skull Icon (5x size, red) ─────────────────────────────
        var skullGO = CreateText(content.transform, "SkullIcon", "\u2620",
            240, FontStyle.Normal, new Color(0.85f, 0.1f, 0.1f, 1f), 280f);
        skullGO.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

        // ── Tight group: Title + Divider + Score (reduced spacing) ────────
        var tightGroup = new GameObject("TitleScoreGroup");
        tightGroup.transform.SetParent(content.transform, false);
        tightGroup.AddComponent<RectTransform>();
        var tightLayout = tightGroup.AddComponent<VerticalLayoutGroup>();
        tightLayout.childAlignment = TextAnchor.MiddleCenter;
        tightLayout.spacing = 2f;
        tightLayout.childControlWidth = true;
        tightLayout.childControlHeight = false;
        tightLayout.childForceExpandWidth = true;
        tightLayout.childForceExpandHeight = false;
        var tightLE = tightGroup.AddComponent<LayoutElement>();
        tightLE.preferredHeight = 100f;
        var tightCSF = tightGroup.AddComponent<ContentSizeFitter>();
        tightCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var titleGO = CreateText(tightGroup.transform, "TitleText", "YOU HAVE FALLEN",
            36, FontStyle.Bold, new Color(0.9f, 0.2f, 0.2f, 1f), 50f);

        var divider = new GameObject("Divider");
        divider.transform.SetParent(tightGroup.transform, false);
        divider.AddComponent<CanvasRenderer>();
        var divImg = divider.AddComponent<Image>();
        divImg.color = Gold;
        var divLE = divider.AddComponent<LayoutElement>();
        divLE.preferredHeight = 3f;
        divLE.flexibleWidth = 1f;

        var scoreGO = CreateText(tightGroup.transform, "ScoreText", "Score: 0",
            24, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f, 1f), 40f);

        // ── Spacer ───────────────────────────────────────────────────────
        var spacer = new GameObject("Spacer");
        spacer.transform.SetParent(content.transform, false);
        spacer.AddComponent<RectTransform>();
        var spacerLE = spacer.AddComponent<LayoutElement>();
        spacerLE.preferredHeight = 10f;

        // ── Gem Revive Button (purple with gold border) ──────────────────
        var (gemBtnGO, gemBtn, gemLabel) = CreateGoldBorderedButton(content.transform,
            "GemReviveButton",
            "\u2666 Revive Here (10 Gems)",
            new Color(0.35f, 0.12f, 0.55f, 1f),   // deep purple
            new Color(0.45f, 0.18f, 0.65f, 1f),    // highlight
            Color.white, 24, 100f);

        // ── Ad Revive Button (teal with gold border) ─────────────────────
        var (adBtnGO, adBtn, adLabel) = CreateGoldBorderedButton(content.transform,
            "AdReviveButton",
            "\u25B6 Watch Ad to Revive",
            new Color(0.1f, 0.4f, 0.45f, 1f),      // dark teal
            new Color(0.15f, 0.5f, 0.55f, 1f),      // highlight
            Color.white, 24, 100f);

        // ── Restart Button (dark with gold border) ───────────────────────
        var (restartBtnGO, restartBtn, restartLabel) = CreateGoldBorderedButton(content.transform,
            "RestartButton",
            "\u21BA Restart from Wave 1",
            new Color(0.22f, 0.22f, 0.25f, 1f),     // dark gray
            new Color(0.32f, 0.32f, 0.35f, 1f),      // highlight
            new Color(0.75f, 0.75f, 0.75f, 1f), 22, 90f);

        // ── Assign to RevivePanel component ──────────────────────────────
        var revivePanels = canvas.GetComponentsInChildren<RevivePanel>(true);
        if (revivePanels.Length == 0)
            revivePanels = canvas.GetComponents<RevivePanel>();

        foreach (var rp in revivePanels)
        {
            var so = new SerializedObject(rp);
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("gemReviveButton").objectReferenceValue = gemBtn;
            so.FindProperty("gemReviveCostText").objectReferenceValue = gemLabel;
            so.FindProperty("adReviveButton").objectReferenceValue = adBtn;
            so.FindProperty("restartButton").objectReferenceValue = restartBtn;
            so.FindProperty("scoreText").objectReferenceValue = scoreGO.GetComponent<Text>();
            so.ApplyModifiedProperties();
        }

        if (revivePanels.Length == 0)
            Debug.LogWarning("RevivePanel component not found on Canvas — panel created but not auto-assigned. Add RevivePanel component and re-run, or assign fields manually.");

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

        Debug.Log("RevivePanel created successfully. " +
            (revivePanels.Length > 0 ? "Fields auto-assigned to RevivePanel component." : "Assign fields manually."));
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static GameObject CreateText(Transform parent, string name, string text,
        int fontSize, FontStyle style, Color color, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;

        return go;
    }

    /// <summary>
    /// Creates a button with a gold border frame around the colored inner fill.
    /// Structure: GoldBorder (Image) → InnerFill (Image + Button) → Text
    /// </summary>
    private static (GameObject, Button, Text) CreateGoldBorderedButton(Transform parent,
        string name, string label, Color bgColor, Color highlightColor,
        Color textColor, int fontSize, float height)
    {
        // Outer gold border frame
        var borderGO = new GameObject(name);
        borderGO.transform.SetParent(parent, false);
        borderGO.AddComponent<CanvasRenderer>();
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color = Gold;
        borderImg.raycastTarget = false;

        var borderLE = borderGO.AddComponent<LayoutElement>();
        borderLE.preferredHeight = height;

        // Inner colored fill (this is the actual button)
        var innerGO = new GameObject("ButtonInner");
        innerGO.transform.SetParent(borderGO.transform, false);
        innerGO.AddComponent<CanvasRenderer>();
        var innerImg = innerGO.AddComponent<Image>();
        innerImg.color = bgColor;
        innerImg.raycastTarget = true;

        var innerRT = innerGO.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(3f, 3f);    // 3px gold border
        innerRT.offsetMax = new Vector2(-3f, -3f);

        var btn = innerGO.AddComponent<Button>();
        btn.targetGraphic = innerImg;

        // Button color transitions
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = highlightColor;
        colors.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
        colors.disabledColor = new Color(bgColor.r * 0.4f, bgColor.g * 0.4f, bgColor.b * 0.4f, 0.6f);
        btn.colors = colors;

        // Button label
        var labelGO = new GameObject("Text");
        labelGO.transform.SetParent(innerGO.transform, false);
        labelGO.AddComponent<CanvasRenderer>();
        var labelText = labelGO.AddComponent<Text>();
        labelText.text = label;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = fontSize;
        labelText.fontStyle = FontStyle.Bold;
        labelText.color = textColor;
        labelText.alignment = TextAnchor.MiddleCenter;

        var labelRT = labelGO.GetComponent<RectTransform>();
        labelRT.anchorMin = Vector2.zero;
        labelRT.anchorMax = Vector2.one;
        labelRT.offsetMin = new Vector2(10f, 5f);
        labelRT.offsetMax = new Vector2(-10f, -5f);

        return (borderGO, btn, labelText);
    }
}
