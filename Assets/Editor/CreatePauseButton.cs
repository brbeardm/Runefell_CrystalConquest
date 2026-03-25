using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreatePauseButton
{
    public static void Execute()
    {
        // Update in current scene (Gameplay_BridgeRange)
        UpdateScene("Assets/Scenes/Gameplay_BridgeRange.unity");
        
        // Update in Gameplay scene
        UpdateScene("Assets/Scenes/Gameplay.unity");
        
        // Reopen the original scene
        EditorSceneManager.OpenScene("Assets/Scenes/Gameplay_BridgeRange.unity", OpenSceneMode.Single);
    }

    private static void UpdateScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError($"Canvas not found in {scene.name}");
            return;
        }
        
        // Check if PauseButton already exists
        Transform existingPauseButton = canvas.transform.Find("PauseButton");
        GameObject pauseButtonObj;
        
        if (existingPauseButton != null)
        {
            pauseButtonObj = existingPauseButton.gameObject;
        }
        else
        {
            // Create new Button
            pauseButtonObj = new GameObject("PauseButton");
            pauseButtonObj.transform.SetParent(canvas.transform, false);
            
            // Add components
            pauseButtonObj.AddComponent<RectTransform>();
            pauseButtonObj.AddComponent<CanvasRenderer>();
            Image image = pauseButtonObj.AddComponent<Image>();
            Button button = pauseButtonObj.AddComponent<Button>();
            
            // Set Image properties
            image.color = new Color(0f, 0f, 0f, 0.5f);
            
            // Create Text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(pauseButtonObj.transform, false);
            
            textObj.AddComponent<RectTransform>();
            textObj.AddComponent<CanvasRenderer>();
            Text text = textObj.AddComponent<Text>();
            
            // Set Text properties
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = "";
            
            // Set Text RectTransform to fill parent
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }
        
        // Set Button RectTransform properties
        RectTransform rect = pauseButtonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(20f, 20f);
        rect.sizeDelta = new Vector2(60f, 60f);
        
        // Assign to UIManager
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            SerializedObject so = new SerializedObject(uiManager);
            
            SerializedProperty pauseButtonProp = so.FindProperty("pauseButton");
            if (pauseButtonProp != null)
            {
                pauseButtonProp.objectReferenceValue = pauseButtonObj.GetComponent<Button>();
            }
            
            SerializedProperty pauseButtonTextProp = so.FindProperty("pauseButtonText");
            if (pauseButtonTextProp != null)
            {
                pauseButtonTextProp.objectReferenceValue = pauseButtonObj.transform.Find("Text").GetComponent<Text>();
            }
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(uiManager);
            Debug.Log($"Assigned PauseButton to UIManager in {scene.name}");
        }
        else
        {
            Debug.LogError($"UIManager not found in {scene.name}");
        }
        
        EditorUtility.SetDirty(canvas);
        EditorSceneManager.SaveScene(scene);
    }
}