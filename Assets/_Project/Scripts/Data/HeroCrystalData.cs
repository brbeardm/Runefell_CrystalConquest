using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroCrystal", menuName = "Runefell/Hero Crystal Data")]
public class HeroCrystalData : ScriptableObject
{
    [Tooltip("Total hits required to free the hero (10 pieces x hitsPerPiece).")]
    public int hitsToFree = 20;

    [Tooltip("Hits required to destroy each crystal piece.")]
    public int hitsPerPiece = 2;

    [Tooltip("How long the hero stays active (seconds).")]
    public float heroDuration = 60f;

    [Tooltip("Damage multiplier for hero projectiles.")]
    public int heroDamageMultiplier = 10;

    [Tooltip("Seconds before crystal respawns after hero expires.")]
    public float respawnDelay = 5f;
}
