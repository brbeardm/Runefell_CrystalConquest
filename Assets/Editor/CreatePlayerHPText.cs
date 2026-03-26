using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CreatePlayerHPText
{
    [MenuItem("Tools/Create Player HP Text")]
    public static void Execute()
    {
        // Find the Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene.");
            return;
        }

        // Create the new GameObject under Canvas
        GameObject go = new GameObject("PlayerHPText");
        go.transform.SetParent(canvas.transform, false);

        // Add required UI components
        go.AddComponent<CanvasRenderer>();
        Text text = go.AddComponent<Text>();

        // Configure Text component
        text.text = "HP: 100/100";
        text.fontSize = 50;
        text.alignment = TextAnchor.UpperRight;
        text.color = Color.white;

        // Use the default Arial font
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Configure RectTransform
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-50f, -150f);
        rt.sizeDelta = new Vector2(400f, 100f);

        // Assign to UIManager.playerHPText
        GameObject canvasGO = canvas.gameObject;
        var uiManager = canvasGO.GetComponent<UIManager>();
        if (uiManager != null)
        {
            var field = typeof(UIManager).GetField("playerHPText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(uiManager, text);
                Debug.Log("Assigned PlayerHPText to UIManager.playerHPText via field.");
            }
            else
            {
                // Try property
                var prop = typeof(UIManager).GetProperty("playerHPText",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    prop.SetValue(uiManager, text);
                    Debug.Log("Assigned PlayerHPText to UIManager.playerHPText via property.");
                }
                else
                {
                    Debug.LogWarning("Could not find playerHPText field or property on UIManager.");
                }
            }
            EditorUtility.SetDirty(uiManager);
        }
        else
        {
            Debug.LogWarning("UIManager component not found on Canvas.");
        }

        // Mark scene dirty and save
        EditorUtility.SetDirty(go);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("PlayerHPText created and scene saved.");
    }
}
