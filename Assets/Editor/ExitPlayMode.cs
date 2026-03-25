using UnityEditor;
using UnityEngine;

public class ExitPlayMode
{
    public static void Execute()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            Debug.Log("Exited play mode.");
        }
    }
}