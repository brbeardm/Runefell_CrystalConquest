using UnityEditor;
using UnityEngine;

public class SetupPlayerTiers
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("---Player---/Player");
        if (player == null)
        {
            Debug.LogError("Player not found.");
            return;
        }

        // Tier0_Base
        GameObject tier0 = new GameObject("Tier0_Base");
        tier0.transform.SetParent(player.transform);
        tier0.transform.localPosition = Vector3.zero;
        tier0.transform.localRotation = Quaternion.identity;
        tier0.transform.localScale = Vector3.one;

        Transform playerBody = player.transform.Find("PlayerBody");
        if (playerBody != null)
        {
            playerBody.SetParent(tier0.transform);
        }

        Transform firePoint = player.transform.Find("FirePoint");
        if (firePoint != null)
        {
            firePoint.SetParent(tier0.transform);
            firePoint.localPosition = new Vector3(0f, 0.5f, -3.5f);
            firePoint.localRotation = Quaternion.identity;
        }
        else
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(tier0.transform);
            fp.transform.localPosition = new Vector3(0f, 0.5f, -3.5f);
            fp.transform.localRotation = Quaternion.identity;
        }

        // Tier1_Hero
        GameObject tier1 = new GameObject("Tier1_Hero");
        tier1.transform.SetParent(player.transform);
        tier1.transform.localPosition = Vector3.zero;
        tier1.transform.localRotation = Quaternion.identity;
        tier1.transform.localScale = Vector3.one;
        tier1.SetActive(false);

        GameObject fp1 = new GameObject("FirePoint");
        fp1.transform.SetParent(tier1.transform);
        fp1.transform.localPosition = new Vector3(0f, 0.5f, -3.5f);
        fp1.transform.localRotation = Quaternion.identity;

        // Tier2_Hero
        GameObject tier2 = new GameObject("Tier2_Hero");
        tier2.transform.SetParent(player.transform);
        tier2.transform.localPosition = Vector3.zero;
        tier2.transform.localRotation = Quaternion.identity;
        tier2.transform.localScale = Vector3.one;
        tier2.SetActive(false);

        GameObject fp2 = new GameObject("FirePoint");
        fp2.transform.SetParent(tier2.transform);
        fp2.transform.localPosition = new Vector3(0f, 0.5f, -3.5f);
        fp2.transform.localRotation = Quaternion.identity;

        // Tier3_Hero
        GameObject tier3 = new GameObject("Tier3_Hero");
        tier3.transform.SetParent(player.transform);
        tier3.transform.localPosition = Vector3.zero;
        tier3.transform.localRotation = Quaternion.identity;
        tier3.transform.localScale = Vector3.one;
        tier3.SetActive(false);

        GameObject fp3 = new GameObject("FirePoint");
        fp3.transform.SetParent(tier3.transform);
        fp3.transform.localPosition = new Vector3(0f, 0.5f, -3.5f);
        fp3.transform.localRotation = Quaternion.identity;

        // Update PlayerShooter firePoint reference
        var shooter = player.GetComponent("PlayerShooter");
        if (shooter != null)
        {
            SerializedObject so = new SerializedObject(shooter);
            so.FindProperty("firePoint").objectReferenceValue = firePoint != null ? firePoint.gameObject : tier0.transform.Find("FirePoint").gameObject;
            so.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(player);
        Debug.Log("Player tiers setup complete.");
    }
}