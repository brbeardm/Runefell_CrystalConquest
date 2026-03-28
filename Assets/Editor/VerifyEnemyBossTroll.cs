using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class VerifyEnemyBossTroll
{
    [MenuItem("Tools/Verify Enemy Boss Troll Prefab")]
    public static void VerifyEnemyBossTrollPrefab()
    {
        // Load the prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemy_Boss_Troll.prefab");
        if (prefab == null)
        {
            Debug.LogError("Failed to load Enemy_Boss_Troll prefab");
            return;
        }

        // Instantiate to check components
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            Debug.LogError("Failed to instantiate prefab");
            return;
        }

        // Verify components
        bool hasEnemy = instance.GetComponent<Enemy>() != null;
        bool hasTrollBossBehavior = instance.GetComponent<TrollBossBehavior>() != null;
        bool hasCapsuleCollider = instance.GetComponent<CapsuleCollider>() != null;
        bool hasRigidbody = instance.GetComponent<Rigidbody>() != null;
        bool hasAnimator = instance.GetComponent<Animator>() != null;

        Debug.Log($"Enemy_Boss_Troll Prefab Verification:");
        Debug.Log($"  - Enemy component: {(hasEnemy ? "✓" : "✗")}");
        Debug.Log($"  - TrollBossBehavior component: {(hasTrollBossBehavior ? "✓" : "✗")}");
        Debug.Log($"  - CapsuleCollider component: {(hasCapsuleCollider ? "✓" : "✗")}");
        Debug.Log($"  - Rigidbody component: {(hasRigidbody ? "✓" : "✗")}");
        Debug.Log($"  - Animator component: {(hasAnimator ? "✓" : "✗")}");

        if (hasAnimator)
        {
            Animator animator = instance.GetComponent<Animator>();
            Debug.Log($"  - Animator Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "None")}");
            Debug.Log($"  - Animator Avatar: {(animator.avatar != null ? animator.avatar.name : "None")}");
        }

        if (hasCapsuleCollider)
        {
            CapsuleCollider collider = instance.GetComponent<CapsuleCollider>();
            Debug.Log($"  - CapsuleCollider Height: {collider.height}");
            Debug.Log($"  - CapsuleCollider Radius: {collider.radius}");
            Debug.Log($"  - CapsuleCollider Center: {collider.center}");
        }

        if (hasRigidbody)
        {
            Rigidbody rb = instance.GetComponent<Rigidbody>();
            Debug.Log($"  - Rigidbody isKinematic: {rb.isKinematic}");
        }

        Debug.Log($"  - Tag: {instance.tag}");

        // Clean up
        Object.DestroyImmediate(instance);
    }
}
