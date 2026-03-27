using UnityEditor;
using UnityEngine;

public class PreviewVictoryPanel
{
    public static void Enable()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Canvas")
            {
                var vp = root.transform.Find("VictoryPanel");
                if (vp != null) vp.gameObject.SetActive(true);
                break;
            }
        }
    }

    public static void Disable()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Canvas")
            {
                var vp = root.transform.Find("VictoryPanel");
                if (vp != null) vp.gameObject.SetActive(false);
                break;
            }
        }
    }
}
