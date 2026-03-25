using UnityEngine;
using UnityEditor;

public class FixShooterPivots
{
    [MenuItem("Tools/Fix Shooter Pivots")]
    public static void Fix()
    {
        // 1. Fix CloneShooter Prefab
        string clonePath = "Assets/_Project/Prefabs/Shooters/CloneShooter.prefab";
        GameObject clonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(clonePath);
        if (clonePrefab != null)
        {
            CapsuleCollider col = clonePrefab.GetComponent<CapsuleCollider>();
            if (col != null && col.center.y == 0)
            {
                float offset = col.height / 2f;
                col.center = new Vector3(col.center.x, offset, col.center.z);

                Transform mesh = clonePrefab.transform.Find("Capsule");
                if (mesh != null)
                {
                    mesh.localPosition = new Vector3(mesh.localPosition.x, mesh.localPosition.y + offset, mesh.localPosition.z);
                }

                Transform firePoint = clonePrefab.transform.Find("FirePoint");
                if (firePoint != null)
                {
                    firePoint.localPosition = new Vector3(firePoint.localPosition.x, firePoint.localPosition.y + offset, firePoint.localPosition.z);
                }

                EditorUtility.SetDirty(clonePrefab);
                PrefabUtility.SavePrefabAsset(clonePrefab);
                Debug.Log("Fixed CloneShooter pivot.");
            }
        }

        // 2. Fix HeroShooter Prefab
        string heroPath = "Assets/_Project/Prefabs/Shooters/HeroShooter.prefab";
        GameObject heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(heroPath);
        if (heroPrefab != null)
        {
            CapsuleCollider col = heroPrefab.GetComponent<CapsuleCollider>();
            if (col != null && col.center.y == 0)
            {
                float offset = col.height / 2f;
                col.center = new Vector3(col.center.x, offset, col.center.z);

                Transform mesh = heroPrefab.transform.Find("Capsule");
                if (mesh != null)
                {
                    mesh.localPosition = new Vector3(mesh.localPosition.x, mesh.localPosition.y + offset, mesh.localPosition.z);
                }

                Transform firePoint = heroPrefab.transform.Find("FirePoint");
                if (firePoint != null)
                {
                    firePoint.localPosition = new Vector3(firePoint.localPosition.x, firePoint.localPosition.y + offset, firePoint.localPosition.z);
                }

                EditorUtility.SetDirty(heroPrefab);
                PrefabUtility.SavePrefabAsset(heroPrefab);
                Debug.Log("Fixed HeroShooter pivot.");
            }
        }

        // 3. Fix Player in Scene
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            CapsuleCollider col = player.GetComponent<CapsuleCollider>();
            if (col != null && col.center.y == 0)
            {
                float offset = col.height / 2f;
                col.center = new Vector3(col.center.x, offset, col.center.z);
                EditorUtility.SetDirty(player);
                Debug.Log("Fixed Player collider center in scene.");
            }
        }

        // 4. Update ShooterManager
        GameObject manager = GameObject.Find("ShooterManager");
        if (manager != null)
        {
            var sm = manager.GetComponent("ShooterManager");
            if (sm != null)
            {
                SerializedObject so = new SerializedObject(sm);
                so.Update();
                so.FindProperty("cloneXSpacing").floatValue = 0.15f;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(manager);
                Debug.Log("Updated ShooterManager cloneXSpacing.");
            }
        }
    }
}
