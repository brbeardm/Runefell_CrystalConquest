using UnityEngine;
using UnityEditor;

public class UpdateHeroCrystalCollider
{
    public static void Execute()
    {
        GameObject heroCrystal = GameObject.Find("HeroCrystal");
        if (heroCrystal != null)
        {
            BoxCollider bc = heroCrystal.GetComponent<BoxCollider>();
            if (bc != null)
            {
                // Bridge is from X=-4 to X=4. HeroCrystal is at X=-3.2.
                // To cover the whole width, center X should be 3.2, size X should be 8.
                bc.center = new Vector3(3.2f, 1.5f, 0f);
                bc.size = new Vector3(8f, 3f, 2f);
                EditorUtility.SetDirty(heroCrystal);
                Debug.Log("Updated HeroCrystal BoxCollider to cover the bridge width.");
            }
            else
            {
                Debug.LogError("BoxCollider not found on HeroCrystal.");
            }
        }
        else
        {
            Debug.LogError("HeroCrystal not found in scene.");
        }
    }
}