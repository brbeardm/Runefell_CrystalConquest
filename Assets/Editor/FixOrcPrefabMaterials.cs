using UnityEditor;
using UnityEngine;

public class FixOrcPrefabMaterials
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            // The FBX child might not be named "enemy_orc" if it was instantiated differently.
            // Let's find the renderers directly.
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            bool foundFBXRenderers = false;
            
            foreach (Renderer r in renderers)
            {
                if (r.name == "Capsule") continue; // Skip the placeholder
                
                foundFBXRenderers = true;
                Material[] mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null)
                    {
                        string matName = mats[i].name;
                        if (matName.EndsWith(" (Instance)"))
                        {
                            matName = matName.Substring(0, matName.Length - 11);
                        }
                        
                        string[] guids = AssetDatabase.FindAssets($"t:Material {matName}", new[] { "Assets/Characters/Materials" });
                        if (guids.Length > 0)
                        {
                            string matPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                            Material newMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                            if (newMat != null)
                            {
                                mats[i] = newMat;
                                Debug.Log($"Assigned {newMat.name} to {r.name}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find material for {matName}");
                        }
                    }
                }
                r.sharedMaterials = mats;
                EditorUtility.SetDirty(r);
            }
            
            if (!foundFBXRenderers)
            {
                Debug.LogWarning("Did not find any FBX renderers in the prefab. Re-adding the FBX.");
                
                string fbxPath = "Assets/Characters/enemy_orc.fbx";
                GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxPrefab != null)
                {
                    GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
                    fbxInstance.transform.SetParent(prefab.transform, false);
                    fbxInstance.transform.localPosition = Vector3.zero;
                    fbxInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                    fbxInstance.transform.localScale = Vector3.one;
                    
                    // Assign Animator Controller
                    Animator animator = fbxInstance.GetComponent<Animator>();
                    if (animator != null)
                    {
                        string controllerPath = "Assets/_Project/Prefabs/Enemies/OrcAnimator.controller";
                        UnityEditor.Animations.AnimatorController controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
                        animator.runtimeAnimatorController = controller;
                    }
                    
                    Debug.Log("Re-added FBX to prefab.");
                }
            }
            
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log("Saved Enemy_Orc prefab.");
        }
    }
}