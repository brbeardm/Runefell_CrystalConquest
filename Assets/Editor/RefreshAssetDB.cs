using UnityEditor;
using UnityEngine;

public static class RefreshAssetDB
{
    public static void Execute()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log("Asset database refreshed.");
    }
}
