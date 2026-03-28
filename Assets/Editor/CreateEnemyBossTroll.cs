using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class CreateEnemyBossTroll
{
    [MenuItem("Tools/Create Enemy Boss Troll Prefab")]
    public static void CreateEnemyBossTrollPrefab()
    {
        // Load the Troll-TPose FBX
        GameObject trollModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Troll-TPose.fbx");
        if (trollModel == null)
        {
            Debug.LogError("Failed to load Troll_TPose.fbx");
            return;
        }

        // Instantiate the model
        GameObject bossTroll = PrefabUtility.InstantiatePrefab(trollModel) as GameObject;
        if (bossTroll == null)
        {
            Debug.LogError("Failed to instantiate Troll_TPose.fbx");
            return;
        }

        bossTroll.name = "Enemy_Boss_Troll";

        // Add Enemy component
        Enemy enemyComponent = bossTroll.AddComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("Failed to add Enemy component");
            Object.DestroyImmediate(bossTroll);
            return;
        }

        // Add TrollBossBehavior component
        TrollBossBehavior bossBehavior = bossTroll.AddComponent<TrollBossBehavior>();
        if (bossBehavior == null)
        {
            Debug.LogError("Failed to add TrollBossBehavior component");
            Object.DestroyImmediate(bossTroll);
            return;
        }

        // Add CapsuleCollider
        CapsuleCollider capsuleCollider = bossTroll.AddComponent<CapsuleCollider>();
        capsuleCollider.height = 2f;
        capsuleCollider.radius = 0.5f;
        capsuleCollider.center = new Vector3(0, 1, 0);

        // Add Rigidbody
        Rigidbody rigidbody = bossTroll.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;

        // Set Tag to "Enemy"
        bossTroll.tag = "Enemy";

        // Configure Animator
        Animator animator = bossTroll.GetComponent<Animator>();
        if (animator != null)
        {
            // Load and assign the TrollController
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/_Project/Animations/TrollController.controller");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning("Failed to load TrollController.controller");
            }

            // Load and assign the avatar from Troll-TPose.fbx
            // The avatar is embedded in the FBX, we need to load all assets and find it
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/Troll-TPose.fbx");
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
            }
            else
            {
                Debug.LogWarning("No Avatar found in Troll_TPose.fbx - the Animator will use the default avatar");
            }

            animator.applyRootMotion = false;
        }
        else
        {
            Debug.LogWarning("No Animator component found on Troll_TPose.fbx - adding one");
            animator = bossTroll.AddComponent<Animator>();

            // Load and assign the TrollController
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/_Project/Animations/TrollController.controller");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            // Load and assign the avatar from Troll-TPose.fbx
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/Troll-TPose.fbx");
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
            }

            animator.applyRootMotion = false;
        }

        // Create the prefab
        string prefabPath = "Assets/_Project/Prefabs/Enemy_Boss_Troll.prefab";
        PrefabUtility.SaveAsPrefabAsset(bossTroll, prefabPath);

        Debug.Log("Enemy_Boss_Troll prefab created successfully at " + prefabPath);

        // Clean up the instance
        Object.DestroyImmediate(bossTroll);

        // Refresh the asset database
        AssetDatabase.Refresh();
    }
}
