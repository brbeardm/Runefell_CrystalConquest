using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CreateEnemyBossOrc
{
    [MenuItem("Tools/Create Enemy Boss Orc Prefab")]
    public static void CreateEnemyBossOrcPrefab()
    {
        // Load the Orc_TPose FBX
        GameObject orcModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Orc_TPose.fbx");
        if (orcModel == null)
        {
            Debug.LogError("Failed to load Orc_TPose.fbx");
            return;
        }

        // Instantiate the model
        GameObject bossOrc = PrefabUtility.InstantiatePrefab(orcModel) as GameObject;
        if (bossOrc == null)
        {
            Debug.LogError("Failed to instantiate Orc_TPose.fbx");
            return;
        }

        bossOrc.name = "Enemy_Boss_Orc";

        // Add Enemy component
        Enemy enemyComponent = bossOrc.AddComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("Failed to add Enemy component");
            Object.DestroyImmediate(bossOrc);
            return;
        }

        // Add OrcBossBehavior component
        OrcBossBehavior bossBehavior = bossOrc.AddComponent<OrcBossBehavior>();
        if (bossBehavior == null)
        {
            Debug.LogError("Failed to add OrcBossBehavior component");
            Object.DestroyImmediate(bossOrc);
            return;
        }

        // Add CapsuleCollider
        CapsuleCollider capsuleCollider = bossOrc.AddComponent<CapsuleCollider>();
        capsuleCollider.height = 2f;
        capsuleCollider.radius = 0.5f;
        capsuleCollider.center = new Vector3(0, 1, 0);

        // Add Rigidbody
        Rigidbody rigidbody = bossOrc.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        // Set Tag to "Enemy"
        bossOrc.tag = "Enemy";

        // Configure Animator
        Animator animator = bossOrc.GetComponent<Animator>();
        if (animator != null)
        {
            // Load and assign the OrcController
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/_Project/Animations/OrcController.controller");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning("Failed to load OrcController.controller");
            }

            // Load and assign the avatar from Orc_TPose.fbx
            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Characters/Orc_TPose.fbx");
            if (avatar != null)
            {
                animator.avatar = avatar;
            }
            else
            {
                Debug.LogWarning("Failed to load avatar from Orc_TPose.fbx");
            }

            animator.applyRootMotion = false;
        }
        else
        {
            Debug.LogWarning("No Animator component found on Orc_TPose.fbx");
        }

        // Create the prefab
        string prefabPath = "Assets/_Project/Prefabs/Enemy_Boss_Orc.prefab";
        PrefabUtility.SaveAsPrefabAsset(bossOrc, prefabPath);

        Debug.Log("Enemy_Boss_Orc prefab created successfully at " + prefabPath);

        // Clean up the instance
        Object.DestroyImmediate(bossOrc);

        // Refresh the asset database
        AssetDatabase.Refresh();
    }
}
