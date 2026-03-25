using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SetupOrcPrefabFix
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Enemies/Enemy_Orc.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError("Could not find Enemy_Orc prefab.");
            return;
        }
        
        // 1. Disable placeholder mesh
        Transform capsule = prefab.transform.Find("Capsule");
        if (capsule != null)
        {
            capsule.gameObject.SetActive(false);
            Debug.Log("Disabled placeholder Capsule mesh.");
        }
        
        // 2. Create Animator Controller
        string controllerPath = "Assets/_Project/Prefabs/Enemies/OrcAnimator.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }
        
        // Find the animation clip in the FBX
        string fbxPath = "Assets/Characters/enemy_orc.fbx";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        AnimationClip walkClip = null;
        
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
            {
                walkClip = clip;
                break;
            }
        }
        
        if (walkClip != null)
        {
            // Check if state already exists
            bool stateExists = false;
            foreach (ChildAnimatorState state in controller.layers[0].stateMachine.states)
            {
                if (state.state.motion == walkClip)
                {
                    stateExists = true;
                    break;
                }
            }
            
            if (!stateExists)
            {
                controller.AddMotion(walkClip);
                Debug.Log($"Added {walkClip.name} to OrcAnimator.");
            }
        }
        else
        {
            Debug.LogWarning("Could not find animation clip in enemy_orc.fbx.");
        }
        
        // 3. Add FBX as child
        GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxPrefab != null)
        {
            // Check if it already exists
            Transform existingFbx = prefab.transform.Find(fbxPrefab.name);
            GameObject fbxInstance;
            
            if (existingFbx != null)
            {
                fbxInstance = existingFbx.gameObject;
            }
            else
            {
                fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
                fbxInstance.transform.SetParent(prefab.transform, false);
            }
            
            // Set transform
            fbxInstance.transform.localPosition = Vector3.zero;
            fbxInstance.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            fbxInstance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Assign Animator Controller
            Animator animator = fbxInstance.GetComponent<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log("Assigned OrcAnimator to FBX child.");
            }
            else
            {
                Debug.LogWarning("No Animator found on FBX child.");
            }
        }
        else
        {
            Debug.LogError("Could not find enemy_orc.fbx prefab.");
        }
        
        // 4. Verify components
        Enemy enemyComp = prefab.GetComponent<Enemy>();
        if (enemyComp == null) Debug.LogWarning("Enemy component missing.");
        
        Collider collider = prefab.GetComponent<Collider>();
        if (collider == null) Debug.LogWarning("Collider missing.");
        
        if (prefab.tag != "Enemy")
        {
            prefab.tag = "Enemy";
            Debug.Log("Set tag to Enemy.");
        }
        
        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log("Saved Enemy_Orc prefab.");
    }
}