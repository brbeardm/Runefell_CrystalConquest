using UnityEngine;

[CreateAssetMenu(fileName = "NewCrystalBall", menuName = "Runefell/Crystal Ball Data")]
public class CrystalBallData : ScriptableObject
{
    [Tooltip("Hits required to crack the crystal ball and clone a shooter.")]
    public int hitsToCrack = 5;

    [Tooltip("Move speed toward the player (units/sec).")]
    public float moveSpeed = 3f;
}
