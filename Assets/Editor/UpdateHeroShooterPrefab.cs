using UnityEditor;
using UnityEngine;

public class UpdateHeroShooterPrefab
{
    public static void Execute()
    {
        string prefabPath = "Assets/_Project/Prefabs/Shooters/HeroShooter.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("Could not load HeroShooter prefab.");
            return;
        }

        // Delete Capsule or ModelMesh
        Transform capsule = prefab.transform.Find("Capsule");
        if (capsule == null) capsule = prefab.transform.Find("ModelMesh");
        
        if (capsule != null)
        {
            Object.DestroyImmediate(capsule.gameObject);
            Debug.Log("Deleted Capsule/ModelMesh from HeroShooter.");
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
            
            Debug.Log("Added PlayerModel to HeroShooter.");

            // Set Material to Mat_HeroShooter
            Material heroMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Shooters/Mat_HeroShooter.mat");
            if (heroMat != null)
            {
                Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in renderers)
                {
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++)
                    {
                        mats[i] = heroMat;
                    }
                    r.sharedMaterials = mats;
                }
                Debug.Log("Applied Mat_HeroShooter to PlayerModel.");
            }
            else
            {
                Debug.LogWarning("Could not find Mat_HeroShooter.mat");
            }

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
            
            Debug.Log("Configured Animator on HeroShooter's PlayerModel.");
        }
        else
        {
            Debug.LogError("Could not find Player1.fbx.");
        }

        // Update CapsuleCollider if needed
        CapsuleCollider collider = prefab.GetComponent<CapsuleCollider>();
        if (collider != null)
        {
            collider.center = new Vector3(0, 0.8f, 0);
            collider.radius = 0.4f;
            collider.height = 1.6f;
            Debug.Log("Updated CapsuleCollider on HeroShooter.");
        }

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        Debug.Log("Successfully updated HeroShooter prefab.");
    }
}