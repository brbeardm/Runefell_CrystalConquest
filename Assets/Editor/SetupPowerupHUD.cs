using UnityEngine;
using UnityEngine.UI;

public class SetupPowerupHUD
{
    [UnityEditor.MenuItem("Tools/Setup PowerupHUD")]
    public static void Execute()
    {
        // Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Create PowerupHUD GameObject
        GameObject powerupHUDGO = new GameObject("PowerupHUD");
        powerupHUDGO.transform.SetParent(canvas.transform, false);

        // Add PowerupHUD script
        PowerupHUD powerupHUD = powerupHUDGO.AddComponent<PowerupHUD>();

        // Find and assign GemCountText (check both Canvas and BottomUILayout)
        Transform gemCountText = canvas.transform.Find("GemCountText");
        if (gemCountText == null)
            gemCountText = canvas.transform.Find("BottomUILayout/GemCountText");
        
        if (gemCountText != null)
        {
            SetPrivateField(powerupHUD, "gemCountText", gemCountText.GetComponent<Text>());
            Debug.Log("Assigned GemCountText");
        }
        else
        {
            Debug.LogWarning("GemCountText not found!");
        }

        // Find and assign RuneCountText (check both Canvas and BottomUILayout)
        Transform runeCountText = canvas.transform.Find("RuneCountText");
        if (runeCountText == null)
            runeCountText = canvas.transform.Find("BottomUILayout/RuneCountText");
        
        if (runeCountText != null)
        {
            SetPrivateField(powerupHUD, "runeCountText", runeCountText.GetComponent<Text>());
            Debug.Log("Assigned RuneCountText");
        }
        else
        {
            Debug.LogWarning("RuneCountText not found!");
        }

        // Find and assign powerupButtons array
        Button[] powerupButtons = new Button[8];
        for (int i = 0; i < 8; i++)
        {
            Transform btnTransform = canvas.transform.Find($"BottomUILayout/PowerupBtn_{i}");
            if (btnTransform != null)
            {
                powerupButtons[i] = btnTransform.GetComponent<Button>();
                Debug.Log($"Assigned PowerupBtn_{i}");
            }
            else
            {
                Debug.LogWarning($"PowerupBtn_{i} not found!");
            }
        }
        SetPrivateField(powerupHUD, "powerupButtons", powerupButtons);

        // Find and assign powerupCostTexts array
        Text[] powerupCostTexts = new Text[8];
        for (int i = 0; i < 8; i++)
        {
            Transform costTextTransform = canvas.transform.Find($"BottomUILayout/PowerupBtn_{i}/CostText");
            if (costTextTransform != null)
            {
                powerupCostTexts[i] = costTextTransform.GetComponent<Text>();
                Debug.Log($"Assigned PowerupBtn_{i}/CostText");
            }
            else
            {
                Debug.LogWarning($"PowerupBtn_{i}/CostText not found!");
            }
        }
        SetPrivateField(powerupHUD, "powerupCostTexts", powerupCostTexts);

        // Find and assign PowerupConfig asset
        string[] configGuids = UnityEditor.AssetDatabase.FindAssets("PowerupConfig t:PowerupConfig");
        if (configGuids.Length > 0)
        {
            string configPath = UnityEditor.AssetDatabase.GUIDToAssetPath(configGuids[0]);
            PowerupConfig config = UnityEditor.AssetDatabase.LoadAssetAtPath<PowerupConfig>(configPath);
            if (config != null)
            {
                SetPrivateField(powerupHUD, "config", config);
                Debug.Log($"Assigned PowerupConfig from {configPath}");
            }
            else
            {
                Debug.LogWarning("PowerupConfig asset found but could not be loaded!");
            }
        }
        else
        {
            Debug.LogWarning("PowerupConfig asset not found in project!");
        }

        Debug.Log("PowerupHUD setup completed!");
    }

    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found on {obj.GetType().Name}");
        }
    }
}
