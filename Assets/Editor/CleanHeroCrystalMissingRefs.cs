using UnityEditor;
using UnityEngine;

public class CleanHeroCrystalMissingRefs
{
    [MenuItem("Tools/Clean HeroCrystal Missing Refs")]
    public static void Execute()
    {
        GameObject heroCrystal = GameObject.Find("HeroCrystal");
        if (heroCrystal == null)
        {
            Debug.LogError("HeroCrystal not found in scene.");
            return;
        }

        // Use SerializedObject to find and remove missing script references
        // on the HeroCrystal component
        Component[] components = heroCrystal.GetComponents<Component>();
        bool cleaned = false;

        SerializedObject so = new SerializedObject(heroCrystal);
        SerializedProperty prop = so.FindProperty("m_Component");

        // Remove any null/missing components via GameObjectUtility
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(heroCrystal);
        if (removed > 0)
        {
            Debug.Log($"Removed {removed} MonoBehaviour(s) with missing scripts from HeroCrystal.");
            cleaned = true;
        }
        else
        {
            Debug.Log("No missing MonoBehaviours found on HeroCrystal.");
        }

        // Also check all children
        foreach (Transform child in heroCrystal.GetComponentsInChildren<Transform>(true))
        {
            int childRemoved = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
            if (childRemoved > 0)
            {
                Debug.Log($"Removed {childRemoved} missing script(s) from {child.gameObject.name}.");
                cleaned = true;
            }
        }

        // Force reimport the asset to pick up new fields
        AssetDatabase.ImportAsset("Assets/_Project/Data/HeroCrystal/DefaultHeroCrystal.asset",
            ImportAssetOptions.ForceUpdate);

        EditorUtility.SetDirty(heroCrystal);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("HeroCrystal cleanup complete. Scene saved.");
    }
}
