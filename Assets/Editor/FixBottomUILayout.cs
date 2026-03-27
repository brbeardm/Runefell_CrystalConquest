using UnityEngine;
using UnityEngine.UI;

public class FixBottomUILayout
{
    [UnityEditor.MenuItem("Tools/Fix Bottom UI Layout")]
    public static void Execute()
    {
        // Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Find the BottomUILayout
        Transform bottomLayout = canvas.transform.Find("BottomUILayout");
        if (bottomLayout == null)
        {
            Debug.LogError("BottomUILayout not found!");
            return;
        }

        // Update the HorizontalLayoutGroup spacing to be tighter
        HorizontalLayoutGroup hlg = bottomLayout.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.spacing = 5; // Tighter spacing
            hlg.padding = new RectOffset(10, 10, 10, 10);
        }

        // Move GemCountText and RuneCountText outside the BottomUILayout to avoid overlap
        Transform gemCountText = bottomLayout.Find("GemCountText");
        Transform runeCountText = bottomLayout.Find("RuneCountText");

        if (gemCountText != null)
        {
            gemCountText.SetParent(canvas.transform, false);
            RectTransform gemRT = gemCountText.GetComponent<RectTransform>();
            gemRT.anchorMin = new Vector2(0, 0);
            gemRT.anchorMax = new Vector2(0, 0);
            gemRT.pivot = new Vector2(0, 0);
            gemRT.anchoredPosition = new Vector2(10, 10);
            gemRT.sizeDelta = new Vector2(150, 100);
        }

        if (runeCountText != null)
        {
            runeCountText.SetParent(canvas.transform, false);
            RectTransform runeRT = runeCountText.GetComponent<RectTransform>();
            runeRT.anchorMin = new Vector2(1, 0);
            runeRT.anchorMax = new Vector2(1, 0);
            runeRT.pivot = new Vector2(1, 0);
            runeRT.anchoredPosition = new Vector2(-10, 10);
            runeRT.sizeDelta = new Vector2(150, 100);
        }

        // Adjust BottomUILayout to not include the text elements
        RectTransform bottomLayoutRT = bottomLayout.GetComponent<RectTransform>();
        bottomLayoutRT.anchorMin = new Vector2(0, 0);
        bottomLayoutRT.anchorMax = new Vector2(1, 0);
        bottomLayoutRT.pivot = new Vector2(0.5f, 0);
        bottomLayoutRT.anchoredPosition = new Vector2(0, 0);
        bottomLayoutRT.sizeDelta = new Vector2(0, 120);

        Debug.Log("Bottom UI Layout fixed successfully!");
    }
}
