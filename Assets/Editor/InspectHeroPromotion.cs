using UnityEditor;
using UnityEngine;

public class InspectHeroPromotion
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            var hp = player.GetComponent("HeroPromotion");
            if (hp != null)
            {
                SerializedObject so = new SerializedObject(hp);
                SerializedProperty tiers = so.FindProperty("tiers");
                Debug.Log("Tiers count: " + tiers.arraySize);
                for (int i = 0; i < tiers.arraySize; i++)
                {
                    SerializedProperty tier = tiers.GetArrayElementAtIndex(i);
                    SerializedProperty visualRoot = tier.FindPropertyRelative("visualRoot");
                    SerializedProperty firePoint = tier.FindPropertyRelative("firePoint");
                    
                    string vrName = visualRoot.objectReferenceValue != null ? visualRoot.objectReferenceValue.name : "null";
                    string fpName = firePoint.objectReferenceValue != null ? firePoint.objectReferenceValue.name : "null";
                    
                    Debug.Log($"Tier {i}: visualRoot={vrName}, firePoint={fpName}");
                }
            }
        }
    }
}