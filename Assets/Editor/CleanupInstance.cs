using UnityEngine;
using UnityEditor;

public class CleanupInstance
{
    [MenuItem("Tools/Cleanup Instance")]
    public static void Cleanup()
    {
        GameObject instance = GameObject.Find("CrystalProjectile_Instance");
        if (instance != null)
        {
            GameObject.DestroyImmediate(instance);
            Debug.Log("Instance cleaned up!");
        }
    }
}
