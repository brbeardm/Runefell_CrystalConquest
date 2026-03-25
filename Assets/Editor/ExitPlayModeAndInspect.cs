using UnityEditor;
using UnityEngine;

public class ExitPlayModeAndInspect
{
    public static void Execute()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            Debug.Log("Exited play mode.");
        }
        else
        {
            Debug.Log("Not in play mode.");
        }
    }
}