using UnityEditor;

public class EnterPlayMode
{
    public static void Execute()
    {
        EditorApplication.isPlaying = true;
    }
}