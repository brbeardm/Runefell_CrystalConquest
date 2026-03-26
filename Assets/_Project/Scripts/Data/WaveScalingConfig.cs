using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewCampaign", menuName = "Runefell/Wave Scaling Config")]
public class WaveScalingConfig : ScriptableObject
{
    [Tooltip("Number of combat waves before victory (default 9).")]
    public int totalWaves = 9;

    [Tooltip("Per-wave settings. Array length must match totalWaves.")]
    public WaveParams[] waves = new WaveParams[9];

    [Serializable]
    public class WaveParams
    {
        public string waveName = "Orc Horde";

        [Header("Orc Flood")]
        [Tooltip("Number of orcs in this wave.")]
        public int orcCount = 10;

        [Tooltip("Orc HP for this wave.")]
        public int orcHP = 30;

        [Tooltip("Orc move speed (units/sec).")]
        public float orcSpeed = 2f;

        [Tooltip("Seconds between orc spawns.")]
        public float orcSpawnInterval = 0.6f;

        [Header("Boss")]
        [Tooltip("Boss HP for this wave.")]
        public int bossHP = 100;

        [Tooltip("Boss move speed (units/sec). Bosses are slower than orcs.")]
        public float bossSpeed = 1f;

        [Tooltip("Seconds into the wave before bosses spawn.")]
        public float bossSpawnDelay = 5f;

        [Header("HeroCrystal")]
        [Tooltip("Hits required to break the crystal this wave.")]
        public int crystalHitsToBreak = 50;

        [Tooltip("Duration of fire rate + damage buff (seconds).")]
        public float buffDuration = 30f;

        [Header("Pacing")]
        [Tooltip("Breather seconds before this wave starts.")]
        public float breatherSeconds = 8f;
    }

    /// <summary>
    /// Returns wave params clamped to valid index.
    /// </summary>
    public WaveParams GetWave(int index)
    {
        if (waves == null || waves.Length == 0) return new WaveParams();
        return waves[Mathf.Clamp(index, 0, waves.Length - 1)];
    }
}
