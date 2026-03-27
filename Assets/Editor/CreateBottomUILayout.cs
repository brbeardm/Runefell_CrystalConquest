using UnityEngine;
using UnityEngine.UI;

public class CreateBottomUILayout
{
    [UnityEditor.MenuItem("Tools/Create Bottom UI Layout")]
    public static void Execute()
    {
        // Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Create main horizontal layout container
        GameObject bottomLayoutGO = new GameObject("BottomUILayout");
        RectTransform bottomLayoutRT = bottomLayoutGO.AddComponent<RectTransform>();
        HorizontalLayoutGroup hlg = bottomLayoutGO.AddComponent<HorizontalLayoutGroup>();
        
        // Set up the layout container
        bottomLayoutRT.SetParent(canvas.transform, false);
        bottomLayoutRT.anchorMin = new Vector2(0, 0);
        bottomLayoutRT.anchorMax = new Vector2(1, 0);
        bottomLayoutRT.pivot = new Vector2(0.5f, 0);
        bottomLayoutRT.anchoredPosition = new Vector2(0, 0);
        bottomLayoutRT.sizeDelta = new Vector2(0, 120);
        
        // Configure horizontal layout
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 10;
        hlg.padding = new RectOffset(10, 10, 10, 10);

        // LEFT SECTION: GemCountText
        GameObject gemCountGO = new GameObject("GemCountText");
        RectTransform gemCountRT = gemCountGO.AddComponent<RectTransform>();
        Text gemCountText = gemCountGO.AddComponent<Text>();
        LayoutElement gemCountLE = gemCountGO.AddComponent<LayoutElement>();
        
        gemCountRT.SetParent(bottomLayoutRT, false);
        gemCountText.text = "Gems: 0";
        gemCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        gemCountText.fontSize = 30;
        gemCountText.fontStyle = FontStyle.Bold;
        gemCountText.color = new Color(0, 0.5f, 1, 1); // Blue
        gemCountText.alignment = TextAnchor.MiddleCenter;
        gemCountLE.preferredWidth = 150;
        gemCountLE.preferredHeight = 100;

        // LEFT POWERUP BUTTONS (0-3)
        for (int i = 0; i < 4; i++)
        {
            CreatePowerupButton(bottomLayoutRT, i);
        }

        // CENTER: HeroCrystal Progress Bar (move existing)
        Transform heroCrystalProgress = canvas.transform.Find("HeroCrystalProgress");
        if (heroCrystalProgress != null)
        {
            heroCrystalProgress.SetParent(bottomLayoutRT, false);
            RectTransform heroCrystalRT = heroCrystalProgress.GetComponent<RectTransform>();
            heroCrystalRT.anchorMin = Vector2.zero;
            heroCrystalRT.anchorMax = Vector2.one;
            heroCrystalRT.offsetMin = Vector2.zero;
            heroCrystalRT.offsetMax = Vector2.zero;
            
            LayoutElement heroCrystalLE = heroCrystalProgress.GetComponent<LayoutElement>();
            if (heroCrystalLE == null)
            {
                heroCrystalLE = heroCrystalProgress.gameObject.AddComponent<LayoutElement>();
            }
            heroCrystalLE.preferredWidth = 300;
            heroCrystalLE.preferredHeight = 100;
        }

        // RIGHT POWERUP BUTTONS (4-7)
        for (int i = 4; i < 8; i++)
        {
            CreatePowerupButton(bottomLayoutRT, i);
        }

        // RIGHT SECTION: RuneCountText
        GameObject runeCountGO = new GameObject("RuneCountText");
        RectTransform runeCountRT = runeCountGO.AddComponent<RectTransform>();
        Text runeCountText = runeCountGO.AddComponent<Text>();
        LayoutElement runeCountLE = runeCountGO.AddComponent<LayoutElement>();
        
        runeCountRT.SetParent(bottomLayoutRT, false);
        runeCountText.text = "Runes: 0";
        runeCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        runeCountText.fontSize = 30;
        runeCountText.fontStyle = FontStyle.Bold;
        runeCountText.color = new Color(1, 0.84f, 0, 1); // Gold
        runeCountText.alignment = TextAnchor.MiddleCenter;
        runeCountLE.preferredWidth = 150;
        runeCountLE.preferredHeight = 100;

        Debug.Log("Bottom UI Layout created successfully!");
    }

    private static void CreatePowerupButton(RectTransform parent, int index)
    {
        // Create button container
        GameObject buttonGO = new GameObject($"PowerupBtn_{index}");
        RectTransform buttonRT = buttonGO.AddComponent<RectTransform>();
        Image buttonImage = buttonGO.AddComponent<Image>();
        Button button = buttonGO.AddComponent<Button>();
        LayoutElement buttonLE = buttonGO.AddComponent<LayoutElement>();
        
        buttonRT.SetParent(parent, false);
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1); // Dark gray
        buttonLE.preferredWidth = 80;
        buttonLE.preferredHeight = 100;

        // Create cost text child
        GameObject costTextGO = new GameObject("CostText");
        RectTransform costTextRT = costTextGO.AddComponent<RectTransform>();
        Text costText = costTextGO.AddComponent<Text>();
        
        costTextRT.SetParent(buttonRT, false);
        costTextRT.anchorMin = Vector2.zero;
        costTextRT.anchorMax = Vector2.one;
        costTextRT.offsetMin = Vector2.zero;
        costTextRT.offsetMax = Vector2.zero;
        
        costText.text = "Cost: 0";
        costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        costText.fontSize = 16;
        costText.fontStyle = FontStyle.Bold;
        costText.color = Color.white;
        costText.alignment = TextAnchor.MiddleCenter;
    }
}
