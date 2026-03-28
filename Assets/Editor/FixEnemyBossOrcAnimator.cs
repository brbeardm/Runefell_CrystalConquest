using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class FixEnemyBossOrcAnimator
{
    [MenuItem("Tools/Fix Enemy Boss Orc Animator")]
    public static void FixEnemyBossOrcAnimatorComponent()
    {
        // Load the prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Boss_Orc.prefab");
        if (prefab == null)
        {
            Debug.LogError("Failed to load Enemy_Boss_Orc prefab");
            return;
        }

        // Instantiate to modify
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            Debug.LogError("Failed to instantiate prefab");
            return;
        }

        // Check if Animator exists, if not add it
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
            Debug.Log("Added Animator component to Enemy_Boss_Orc");
        }

        // Load and assign the OrcController
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/_Project/Animations/OrcController.controller");
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log("Assigned OrcController to Animator");
        }
        else
        {
            Debug.LogError("Failed to load OrcController.controller");
        }

        // Load and assign the avatar from Orc_TPose.fbx
        // The avatar is embedded in the FBX, we need to load all assets and find it
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/Orc_TPose.fbx");
        Avatar avatar = null;
        foreach (Object asset in allAssets)
        {
            if (asset is Avatar)
            {
                avatar = asset as Avatar;
                break;
            }
        }

        if (avatar != null)
        {
            animator.avatar = avatar;
            Debug.Log("Assigned Avatar from Orc_TPose.fbx to Animator");
        }
        else
        {
            Debug.LogWarning("No Avatar found in Orc_TPose.fbx - the Animator will use the default avatar");
        }

        animator.applyRootMotion = false;

        // Save the prefab
        PrefabUtility.SaveAsPrefabAsset(instance, "Assets/_Project/Prefabs/Enemy_Boss_Orc.prefab");
        Debug.Log("Enemy_Boss_Orc prefab updated with Animator configuration");

        // Clean up
        Object.DestroyImmediate(instance);

        // Refresh the asset database
        AssetDatabase.Refresh();
    }
}
