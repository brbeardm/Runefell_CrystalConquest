using UnityEngine;
using UnityEditor;

public class FindOrcAvatar
{
    [MenuItem("Tools/Find Orc Avatar")]
    public static void FindOrcAvatarAsset()
    {
        // Try different paths to find the avatar
        string[] possiblePaths = new string[]
        {
            "Assets/Characters/Orc_TPose.fbx",
            "Assets/Characters/Orc_TPose@Avatar",
            "Assets/Characters/Orc_TPose/Avatar"
        };

        foreach (string path in possiblePaths)
        {
            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
            if (avatar != null)
            {
                Debug.Log($"Found avatar at: {path}");
                return;
            }
        }

        // Try to load all assets from the FBX
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Characters/Orc_TPose.fbx");
        Debug.Log($"Total assets in Orc_TPose.fbx: {allAssets.Length}");
        
        foreach (Object asset in allAssets)
        {
            Debug.Log($"  - {asset.name} ({asset.GetType().Name})");
            if (asset is Avatar)
            {
                Debug.Log($"    ^ This is an Avatar!");
            }
        }
    }
}
