using UnityEditor;
using UnityEngine;

public class AddPlayerAnimationDriver
{
    public static void Execute()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            var driver = player.GetComponent("PlayerAnimationDriver");
            if (driver == null)
            {
                player.AddComponent(System.Type.GetType("PlayerAnimationDriver, Assembly-CSharp"));
                Debug.Log("Added PlayerAnimationDriver to Player.");
            }
            else
            {
                Debug.Log("PlayerAnimationDriver already exists on Player.");
            }
        }
        else
        {
            Debug.LogError("Could not find Player GameObject.");
        }
    }
}