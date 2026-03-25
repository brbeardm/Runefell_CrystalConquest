using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FixHeroTiersOffset
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
                    // Start from index 1 (skip Tier 0)
                    for (int i = 1; i < tiersProp.arraySize; i++)
                    {
                        SerializedProperty tierProp = tiersProp.GetArrayElementAtIndex(i);
                        SerializedProperty visualRootProp = tierProp.FindPropertyRelative("visualRoot");
                        
                        if (visualRootProp != null && visualRootProp.objectReferenceValue != null)
                        {
                            GameObject visualRoot = visualRootProp.objectReferenceValue as GameObject;
                            
                            // Ensure visual root local position is (0,0,0)
                            if (visualRoot.transform.localPosition != Vector3.zero)
                            {
                                Debug.Log($"Resetting local position of {visualRoot.name} to (0,0,0) in {scene.name}");
                                visualRoot.transform.localPosition = Vector3.zero;
                                EditorUtility.SetDirty(visualRoot);
                            }
                            
                            // Find the mesh child (usually named "PlayerBody" or similar)
                            // Let's just look for any child that has a MeshRenderer or SkinnedMeshRenderer
                            // Or just check all children except FirePoint
                            foreach (Transform child in visualRoot.transform)
                            {
                                if (child.name != "FirePoint")
                                {
                                    // Move the mesh child inside the visual root so the model's feet/center aligns with local (0,0,0)
                                    // The base player mesh was fixed by setting its local Z to 0.
                                    Vector3 pos = child.localPosition;
                                    if (pos.z != 0f)
                                    {
                                        Debug.Log($"Fixing offset of {child.name} in {visualRoot.name} in {scene.name}. Old pos: {pos}");
                                        child.localPosition = new Vector3(pos.x, pos.y, 0f);
                                        EditorUtility.SetDirty(child.gameObject);
                                    }
                                }
                                else if (child.name == "FirePoint")
                                {
                                    // FirePoint was at -3.5 when PlayerBody was at -4. So it should be at 0.5.
                                    Vector3 pos = child.localPosition;
                                    if (pos.z != 0.5f)
                                    {
                                        Debug.Log($"Fixing offset of FirePoint in {visualRoot.name} in {scene.name}. Old pos: {pos}");
                                        child.localPosition = new Vector3(pos.x, pos.y, 0.5f);
                                        EditorUtility.SetDirty(child.gameObject);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        EditorSceneManager.SaveScene(scene);
    }
}