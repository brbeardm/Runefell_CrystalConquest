using UnityEditor;
using UnityEngine;

public class SetupGroundLayer
{
    public static void Execute()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        int groundIndex = -1;
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == "Ground")
            {
                groundIndex = i;
                break;
            }
        }
        
        if (groundIndex > 7)
        {
            int targetIndex = -1;
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(6).stringValue)) targetIndex = 6;
            else if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(7).stringValue)) targetIndex = 7;
            
            if (targetIndex != -1)
            {
                layers.GetArrayElementAtIndex(targetIndex).stringValue = "Ground";
                layers.GetArrayElementAtIndex(groundIndex).stringValue = "";
                tagManager.ApplyModifiedProperties();
                Debug.Log("Moved 'Ground' layer to index " + targetIndex);
            }
        }
        
        GameObject bridgeFloor = GameObject.Find("---World---/BridgeFloor");
        if (bridgeFloor != null)
        {
            int groundLayer = LayerMask.NameToLayer("Ground");
            bridgeFloor.layer = groundLayer;
            Debug.Log("Assigned 'Ground' layer to " + bridgeFloor.name);
        }
    }
}