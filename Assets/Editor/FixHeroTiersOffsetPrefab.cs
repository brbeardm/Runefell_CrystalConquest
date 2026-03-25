using UnityEditor;
using UnityEngine;

public class FixHeroTiersOffsetPrefab
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/HeroShooter.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            HeroPromotion promotion = prefab.GetComponent<HeroPromotion>();
            if (promotion != null)
            {
                SerializedObject so = new SerializedObject(promotion);
                SerializedProperty tiersProp = so.FindProperty("tiers");
                
                if (tiersProp != null)
                {
                    for (int i = 1; i < tiersProp.arraySize; i++)
                    {
                        SerializedProperty tierProp = tiersProp.GetArrayElementAtIndex(i);
                        SerializedProperty visualRootProp = tierProp.FindPropertyRelative("visualRoot");
                        
                        if (visualRootProp != null && visualRootProp.objectReferenceValue != null)
                        {
                            GameObject visualRoot = visualRootProp.objectReferenceValue as GameObject;
                            
                            if (visualRoot.transform.localPosition != Vector3.zero)
                            {
                                visualRoot.transform.localPosition = Vector3.zero;
                            }
                            
                            foreach (Transform child in visualRoot.transform)
                            {
                                if (child.name != "FirePoint")
                                {
                                    Vector3 pos = child.localPosition;
                                    if (pos.z != 0f)
                                    {
                                        Debug.Log($"Fixing offset of {child.name} in {visualRoot.name} in prefab. Old pos: {pos}");
                                        child.localPosition = new Vector3(pos.x, pos.y, 0f);
                                    }
                                }
                                else if (child.name == "FirePoint")
                                {
                                    Vector3 pos = child.localPosition;
                                    if (pos.z != 0.5f)
                                    {
                                        Debug.Log($"Fixing offset of FirePoint in {visualRoot.name} in prefab. Old pos: {pos}");
                                        child.localPosition = new Vector3(pos.x, pos.y, 0.5f);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
        }
    }
}