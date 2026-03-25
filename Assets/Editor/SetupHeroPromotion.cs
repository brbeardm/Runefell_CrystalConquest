using UnityEditor;
using UnityEngine;

public class SetupHeroPromotion
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("---Player---/Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return;
        }

        HeroPromotion heroPromotion = player.GetComponent<HeroPromotion>();
        if (heroPromotion == null)
        {
            heroPromotion = player.AddComponent<HeroPromotion>();
        }

        SerializedObject so = new SerializedObject(heroPromotion);
        
        // Set playerShooter
        PlayerShooter playerShooter = player.GetComponent<PlayerShooter>();
        so.FindProperty("playerShooter").objectReferenceValue = playerShooter;

        // Set tiers array
        SerializedProperty tiersProp = so.FindProperty("tiers");
        tiersProp.arraySize = 4;

        // Tier 0
        SerializedProperty tier0 = tiersProp.GetArrayElementAtIndex(0);
        tier0.FindPropertyRelative("visualRoot").objectReferenceValue = player.transform.Find("Tier0_Base").gameObject;
        tier0.FindPropertyRelative("firePoint").objectReferenceValue = player.transform.Find("Tier0_Base/FirePoint").gameObject;
        tier0.FindPropertyRelative("fireRate").floatValue = 6f;
        tier0.FindPropertyRelative("damage").intValue = 10;
        tier0.FindPropertyRelative("duration").floatValue = 0f;

        // Tier 1
        SerializedProperty tier1 = tiersProp.GetArrayElementAtIndex(1);
        tier1.FindPropertyRelative("visualRoot").objectReferenceValue = player.transform.Find("Tier1_Hero").gameObject;
        tier1.FindPropertyRelative("firePoint").objectReferenceValue = player.transform.Find("Tier1_Hero/FirePoint").gameObject;
        tier1.FindPropertyRelative("fireRate").floatValue = 8f;
        tier1.FindPropertyRelative("damage").intValue = 30;
        tier1.FindPropertyRelative("duration").floatValue = 60f;

        // Tier 2
        SerializedProperty tier2 = tiersProp.GetArrayElementAtIndex(2);
        tier2.FindPropertyRelative("visualRoot").objectReferenceValue = player.transform.Find("Tier2_Hero").gameObject;
        tier2.FindPropertyRelative("firePoint").objectReferenceValue = player.transform.Find("Tier2_Hero/FirePoint").gameObject;
        tier2.FindPropertyRelative("fireRate").floatValue = 10f;
        tier2.FindPropertyRelative("damage").intValue = 60;
        tier2.FindPropertyRelative("duration").floatValue = 60f;

        // Tier 3
        SerializedProperty tier3 = tiersProp.GetArrayElementAtIndex(3);
        tier3.FindPropertyRelative("visualRoot").objectReferenceValue = player.transform.Find("Tier3_Hero").gameObject;
        tier3.FindPropertyRelative("firePoint").objectReferenceValue = player.transform.Find("Tier3_Hero/FirePoint").gameObject;
        tier3.FindPropertyRelative("fireRate").floatValue = 12f;
        tier3.FindPropertyRelative("damage").intValue = 100;
        tier3.FindPropertyRelative("duration").floatValue = 60f;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(player);
        Debug.Log("HeroPromotion component added and configured.");
    }
}