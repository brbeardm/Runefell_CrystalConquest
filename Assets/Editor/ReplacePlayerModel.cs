using UnityEditor;
using UnityEngine;

public class ReplacePlayerModel
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Could not find Player GameObject.");
            return;
        }

        // Find Tier0_Base
        Transform tier0Base = player.transform.Find("Tier0_Base");
        if (tier0Base == null)
        {
            Debug.LogError("Could not find Tier0_Base.");
            return;
        }

        // Delete PlayerBody (the capsule)
        Transform playerBody = tier0Base.Find("PlayerBody");
        if (playerBody != null)
        {
            Undo.DestroyObjectImmediate(playerBody.gameObject);
            Debug.Log("Deleted PlayerBody (Capsule).");
        }

        // Instantiate Player1.fbx
        GameObject fbxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Characters/Player1.fbx");
        if (fbxPrefab != null)
        {
            GameObject playerModel = (GameObject)PrefabUtility.InstantiatePrefab(fbxPrefab);
            playerModel.name = "PlayerModel";
            
            // The prompt says "onto the Player GameObject in the Hierarchy to add it as a child"
            // But to work with HeroPromotion, it should probably be under Tier0_Base.
            // Let's put it under Tier0_Base.
            playerModel.transform.SetParent(tier0Base, false);
            playerModel.transform.localPosition = Vector3.zero;
            playerModel.transform.localRotation = Quaternion.identity;
            playerModel.transform.localScale = Vector3.one; // Adjust if needed
            
            Undo.RegisterCreatedObjectUndo(playerModel, "Create PlayerModel");
            Debug.Log("Added PlayerModel to Tier0_Base.");

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
            
            Debug.Log("Configured Animator on PlayerModel.");
        }
        else
        {
            Debug.LogError("Could not find Player1.fbx.");
        }

        // Fix FirePoint
        Transform firePoint = tier0Base.Find("FirePoint");
        if (firePoint != null)
        {
            firePoint.localPosition = new Vector3(0, 1.5f, 0.8f);
            Debug.Log("Updated FirePoint position.");
        }
        else
        {
            Debug.LogError("Could not find FirePoint.");
        }
    }
}