using UnityEngine;
using UnityEditor;

public class CheckCollisionMatrix
{
    public static void Execute()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        
        bool ignore = Physics.GetIgnoreLayerCollision(playerLayer, enemyLayer);
        Debug.Log($"Player ({playerLayer}) and Enemy ({enemyLayer}) ignore collision: {ignore}");
    }
}
