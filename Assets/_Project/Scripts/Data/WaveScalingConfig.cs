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

        [Header("Orc Stats")]
        [Tooltip("Orc HP for this wave.")]
        public int orcHP = 30;

        [Tooltip("Orc move speed (units/sec).")]
        public float orcSpeed = 2f;

        [Header("Adaptive Spawn Budget")]
        [Tooltip("Total orcs this wave can spawn. Wave ends when budget is spent and all are dead.")]
        public int orcBudget = 30;

        [Tooltip("Fraction of budget that must be spent before boss can trigger (0-1).")]
        [Range(0f, 1f)]
        public float bossUnlockThreshold = 0.6f;

        [Header("Tension Curve")]
        [Tooltip("Number of Build-Surge-Relief oscillations before boss entrance.")]
        public int tensionCycles = 2;

        [Tooltip("Target pressure during Build phase (ramps from x to y).")]
        public Vector2 buildPressure = new Vector2(30f, 65f);

        [Tooltip("Target pressure during Surge phase.")]
        public Vector2 surgePressure = new Vector2(80f, 85f);

        [Tooltip("Target pressure during Relief phase.")]
        public Vector2 reliefPressure = new Vector2(30f, 40f);

        [Tooltip("Pressure must drop below this for boss entrance after cycles complete.")]
        public float bossEntrancePressure = 25f;

        [Header("Phase Durations (seconds)")]
        public float buildDuration = 8f;
        public float surgeDuration = 6f;
        public float reliefDuration = 5f;

        [Header("Boss")]
        [Tooltip("Boss HP for this wave.")]
        public int bossHP = 100;

        [Tooltip("Boss move speed (units/sec). Bosses are slower than orcs.")]
        public float bossSpeed = 1f;

        [Header("HeroCrystal")]
        [Tooltip("Hits required to break the crystal this wave.")]
        public int crystalHitsToBreak = 50;

        [Tooltip("Duration of fire rate + damage buff (seconds).")]
        public float buffDuration = 30f;

        [Header("Pacing")]
        [Tooltip("Breather seconds before this wave starts.")]
        public float breatherSeconds = 3f;

        // Legacy fields kept hidden so existing .asset files don't lose data
        [HideInInspector] public int orcCount;
        [HideInInspector] public float orcSpawnInterval;
        [HideInInspector] public float bossSpawnDelay;
    }

    public WaveParams GetWave(int index)
    {
        if (waves == null || waves.Length == 0) return new WaveParams();
        return waves[Mathf.Clamp(index, 0, waves.Length - 1)];
    }
}
