using UnityEditor;
using UnityEngine;

public class SetupHeroModels
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("---Player---/Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return;
        }

        // Load HeroMeshInside from HeroCrystal prefab
        string crystalPrefabPath = "Assets/_Project/Prefabs/HeroCrystal/HeroCrystal.prefab";
        GameObject crystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(crystalPrefabPath);
        if (crystalPrefab == null)
        {
            Debug.LogError("HeroCrystal prefab not found.");
            return;
        }

        Transform heroMeshInsidePrefab = crystalPrefab.transform.Find("HeroMeshInside");
        if (heroMeshInsidePrefab == null)
        {
            Debug.LogError("HeroMeshInside not found in HeroCrystal prefab.");
            return;
        }

        // Load Mat_HeroShooter
        Material heroMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Prefabs/Shooters/Mat_HeroShooter.mat");
        if (heroMat == null)
        {
            Debug.LogError("Mat_HeroShooter not found.");
            return;
        }

        string[] tierNames = { "Tier1_Hero", "Tier2_Hero", "Tier3_Hero" };

        foreach (string tierName in tierNames)
        {
            Transform tier = player.transform.Find(tierName);
            if (tier != null)
            {
                // Check if HeroModel already exists
                Transform existingModel = tier.Find("HeroModel");
                if (existingModel != null)
                {
                    Object.DestroyImmediate(existingModel.gameObject);
                }

                // Instantiate HeroModel
                GameObject heroModel = Object.Instantiate(heroMeshInsidePrefab.gameObject, tier);
                heroModel.name = "HeroModel";

                // Scale to match base player footprint
                heroModel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

                // Position so the body center matches PlayerBody center (0, 0.5, -4)
                heroModel.transform.localPosition = new Vector3(0f, -0.1f, -4f);

                // Face forward
                heroModel.transform.localRotation = Quaternion.identity;

                // Remove colliders and assign opaque material
                Collider[] colliders = heroModel.GetComponentsInChildren<Collider>(true);
                foreach (Collider col in colliders)
                {
                    Object.DestroyImmediate(col);
                }

                MeshRenderer[] renderers = heroModel.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer rend in renderers)
                {
                    rend.sharedMaterial = heroMat;
                }

                // Ensure tier is inactive
                tier.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError(tierName + " not found.");
            }
        }

        // Ensure Tier0_Base is active
        Transform tier0 = player.transform.Find("Tier0_Base");
        if (tier0 != null)
        {
            tier0.gameObject.SetActive(true);
        }

        EditorUtility.SetDirty(player);
        Debug.Log("Hero models placed in tiers successfully.");
    }
}