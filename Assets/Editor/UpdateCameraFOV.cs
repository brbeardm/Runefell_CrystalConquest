using UnityEditor;
using UnityEngine;

public class UpdateCameraFOV
{
    public static void Execute()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.fieldOfView = 80f;
            EditorUtility.SetDirty(cam);
            Debug.Log("Set Main Camera FOV to 80.");
        }
    }
}