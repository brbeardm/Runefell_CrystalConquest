using UnityEditor;
using UnityEngine;

public class UpdateCloneShooterPrefab
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("Could not load CloneShooter prefab.");
            return;
        }

        // Delete ModelMesh
        Transform modelMesh = prefab.transform.Find("ModelMesh");
        if (modelMesh != null)
        {
            Object.DestroyImmediate(modelMesh.gameObject);
            Debug.Log("Deleted ModelMesh from CloneShooter.");
        }

        // Add Player1.fbx
        GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Player1.fbx");
        if (fbxPrefab != null)
        {
            GameObject playerModel = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
            playerModel.name = "PlayerModel";
            playerModel.transform.SetParent(prefab.transform, false);
            playerModel.transform.localPosition = Vector3.zero;
            playerModel.transform.localRotation = Quaternion.identity;
            playerModel.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // Match Player in scene
            
            Debug.Log("Added PlayerModel to CloneShooter.");

            // Add Animator component
            Animator animator = playerModel.GetComponent<Animator>();
            if (animator == null)
            {
                animator = playerModel.AddComponent<Animator>();
            }

            // Assign Controller
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Prefabs/Shooters/PlayerAnimator.controller");
            animator.runtimeAnimatorController = controller;

            // Assign Avatar
            Avatar avatar = null;
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/Player1.fbx");
            foreach (Object asset in assets)
            {
                if (asset is Avatar)
                {
                    avatar = asset as Avatar;
                    break;
                }
            }
            animator.avatar = avatar;
            
            Debug.Log("Configured Animator on CloneShooter's PlayerModel.");
        }
        else
        {
            Debug.LogError("Could not find Player1.fbx.");
        }

        // Update CapsuleCollider
        CapsuleCollider collider = prefab.GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            collider.center = new Vector3(0, 0.8f, 0);
            collider.radius = 0.4f;
            collider.height = 1.6f;
            Debug.Log("Updated CapsuleCollider on CloneShooter.");
        }
        else
        {
            Debug.LogWarning("No CapsuleCollider found on CloneShooter root.");
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        Debug.Log("Successfully updated CloneShooter prefab.");
    }
}