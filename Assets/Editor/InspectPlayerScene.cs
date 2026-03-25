using UnityEngine;
using UnityEditor;

public class InspectPlayerScene
{
    [MenuItem("Tools/Inspect Player Scene")]
    public static void Inspect()
    {
        GameObject player = GameObject.Find("---Player---/Player");
        if (player != null)
        {
            Debug.Log($"--- {player.name} ---");
            foreach (Transform child in player.transform)
            {
                Debug.Log($"Child: {child.name}, LocalPos: {child.localPosition}");
                foreach (Transform grandChild in child)
                {
                    Debug.Log($"  GrandChild: {grandChild.name}, LocalPos: {grandChild.localPosition}");
                }
            }
            CapsuleCollider col = player.GetComponent<CapsuleCollider>();
            if (col != null)
            {
                Debug.Log($"Collider Center: {col.center}, Height: {col.height}");
            }
        }
    }
}
