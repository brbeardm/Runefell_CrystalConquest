using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixHeroPromotionTiers
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
        
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            HeroPromotion promotion = player.GetComponent<HeroPromotion>();
            if (promotion != null)
            {
                SerializedObject so = new SerializedObject(promotion);
                SerializedProperty tiersProp = so.FindProperty("tiers");
                
                if (tiersProp != null)
                {
                    for (int i = 0; i < tiersProp.arraySize; i++)
                    {
                        SerializedProperty tierProp = tiersProp.GetArrayElementAtIndex(i);
                        SerializedProperty visualRootProp = tierProp.FindPropertyRelative("visualRoot");
                        
                        if (visualRootProp != null && visualRootProp.objectReferenceValue != null)
                        {
                            GameObject visualRoot = visualRootProp.objectReferenceValue as GameObject;
                            
                            // Check if it's a child of the player
                            if (visualRoot.transform.parent != player.transform)
                            {
                                Debug.Log($"Reparenting {visualRoot.name} to {player.name} in {scene.name}");
                                visualRoot.transform.SetParent(player.transform);
                            }
                            
                            // Ensure local position is (0,0,0)
                            if (visualRoot.transform.localPosition != Vector3.zero)
                            {
                                Debug.Log($"Resetting local position of {visualRoot.name} to (0,0,0) in {scene.name}");
                                visualRoot.transform.localPosition = Vector3.zero;
                            }
                            
                            EditorUtility.SetDirty(visualRoot);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"HeroPromotion component not found on {player.name} in {scene.name}");
            }
        }
        else
        {
            Debug.LogWarning($"Player GameObject not found in {scene.name}");
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}