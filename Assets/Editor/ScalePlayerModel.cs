using UnityEditor;
using UnityEngine;

public class ScalePlayerModel
{
    public static void Execute()
    {
        GameObject playerModel = GameObject.Find("PlayerModel");
        if (playerModel != null)
        {
            playerModel.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Debug.Log("Scaled PlayerModel to 0.1");
        }
    }
}