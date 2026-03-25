using UnityEditor;
using UnityEngine;

public class UpdateHeroCrystalColliderTight
{
    public static void Execute()
    {
        GameObject heroCrystal = GameObject.Find("HeroCrystal");
        if (heroCrystal != null)
        {
            BoxCollider bc = heroCrystal.GetComponent<BoxCollider>();
            if (bc != null)
            {
                // Set local center X to 0, and size X to 1.6
                bc.center = new Vector3(0f, 1.5f, 0f);
                bc.size = new Vector3(1.6f, 3f, 2f);
                
                // Also ensure the HeroCrystal is positioned correctly at X = -3.2 so it matches the -4.0 to -2.4 range
                Vector3 pos = heroCrystal.transform.position;
                pos.x = -3.2f;
                heroCrystal.transform.position = pos;

                EditorUtility.SetDirty(heroCrystal);
                Debug.Log("Updated HeroCrystal BoxCollider to tightly wrap the crystal (Center X=0, Size X=1.6) and set X position to -3.2.");
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