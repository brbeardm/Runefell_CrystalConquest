using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroCrystal", menuName = "Runefell/Hero Crystal Data")]
public class HeroCrystalData : ScriptableObject
{
    [Tooltip("Base hits required to break the crystal (overridden per wave by WaveScalingConfig).")]
    public int hitsToFree = 50;

    [Tooltip("Hits required to destroy each crystal piece.")]
    public int hitsPerPiece = 5;

    [Header("Buff")]
    [Tooltip("Fire rate multiplier when buff is active.")]
    public float fireRateMultiplier = 2f;

    [Tooltip("Damage multiplier when buff is active.")]
    public float damageMultiplier = 2f;

    [Header("Respawn")]
    [Tooltip("Seconds before crystal respawns after buff expires.")]
    public float respawnDelay = 10f;
}
