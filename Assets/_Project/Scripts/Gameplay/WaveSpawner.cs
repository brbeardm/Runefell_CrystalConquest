using UnityEngine;
using System;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Serializable]
    public class EnemyGroup
    {
        [Tooltip("Enemy prefab to spawn.")]
        public GameObject enemyPrefab;

        [Tooltip("How many of this enemy to spawn in the wave.")]
        public int count = 10;

        [Tooltip("Seconds between each spawn in this group.")]
        public float spawnInterval = 0.6f;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Wave 1";

        [Tooltip("Enemy groups in this wave. Multiple groups spawn concurrently.")]
        public EnemyGroup[] enemyGroups;

        [Tooltip("Seconds to wait before the next wave begins after this one is cleared.")]
        public float delayAfterWave = 3f;
    }

    [Header("Wave Configuration")]
    [SerializeField] private WaveDefinition[] waves;

    [Tooltip("After all defined waves are done, keep looping with increasing difficulty.")]
    [SerializeField] private bool endlessMode = true;

    [Tooltip("Each endless loop multiplies enemy count by this factor.")]
    [SerializeField] private float endlessCountMultiplier = 1.3f;

    [Tooltip("Each endless loop multiplies enemy speed by this factor (via spawn interval reduction).")]
    [SerializeField] private float endlessSpeedMultiplier = 0.9f;

    [Header("Bridge Bounds (enemy horde zone only)")]
    [Tooltip("Minimum X position for enemy spawns (horde zone start).")]
    [SerializeField] private float bridgeMinX = -2.4f;

    [Tooltip("Maximum X position for enemy spawns (horde zone end).")]
    [SerializeField] private float bridgeMaxX = 3.2f;

    [Header("Spawn Settings")]
    [Tooltip("Where enemies appear. Only the Z and Y of this transform are used. X is randomized within bridge bounds.")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Fallback spawn Z if no spawnPoint is assigned. Should be the far end of the bridge.")]
    [SerializeField] private float fallbackSpawnZ = 50f;

    [Tooltip("Spawn height above bridge surface.")]
    [SerializeField] private float spawnY = 0.5f;

    [Tooltip("Start spawning automatically on scene load.")]
    [SerializeField] private bool autoStart = true;

    [Tooltip("Seconds before the very first wave begins.")]
    [SerializeField] private float initialDelay = 3f;

    // ── Events ──────────────────────────────────────────
    public static event Action<int, string> OnWaveStarted;      // waveIndex, waveName
    public static event Action<int> OnWaveCompleted;             // waveIndex
    public static event Action OnAllWavesCompleted;

    // ── Runtime state ───────────────────────────────────
    private int _currentWaveIndex = -1;
    private int _enemiesAlive;
    private bool _spawning;
    private bool _finished;
    private int _endlessLoopCount;

    public int CurrentWave => _currentWaveIndex;
    public int TotalWaves => waves != null ? waves.Length : 0;
    public int EnemiesAlive => _enemiesAlive;
    public bool IsFinished => _finished;

    private void OnEnable()
    {
        Enemy.OnEnemyDied += HandleEnemyDied;
        GameManager.OnGameStarted += HandleGameStarted;
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
        GameManager.OnGameStarted -= HandleGameStarted;
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        if (autoStart)
            StartWaves();
    }

    private void HandleGameStarted()
    {
        if (!_spawning)
            StartWaves();
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();
        _spawning = false;
    }

    public void StartWaves()
    {
        if (_spawning) return;

        _currentWaveIndex = -1;
        _endlessLoopCount = 0;
        _finished = false;
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(initialDelay);

        // Run defined waves first
        for (int w = 0; w < waves.Length; w++)
        {
            yield return RunSingleWave(w, waves[w], 1f, 1f);

            if (w < waves.Length - 1)
                yield return new WaitForSeconds(waves[w].delayAfterWave);
        }

        // Endless mode: keep looping through defined waves with scaling difficulty
        if (endlessMode && waves.Length > 0)
        {
            while (true)
            {
                _endlessLoopCount++;
                float countScale = Mathf.Pow(endlessCountMultiplier, _endlessLoopCount);
                float intervalScale = Mathf.Pow(endlessSpeedMultiplier, _endlessLoopCount);

                for (int w = 0; w < waves.Length; w++)
                {
                    int globalWaveIndex = waves.Length * _endlessLoopCount + w;
                    yield return RunSingleWave(globalWaveIndex, waves[w], countScale, intervalScale);
                    yield return new WaitForSeconds(Mathf.Max(1f, waves[w].delayAfterWave * intervalScale));
                }
            }
        }
        else
        {
            _finished = true;
            OnAllWavesCompleted?.Invoke();
        }
    }

    private IEnumerator RunSingleWave(int waveIndex, WaveDefinition wave, float countScale, float intervalScale)
    {
        _currentWaveIndex = waveIndex;

        string name = endlessMode && _endlessLoopCount > 0
            ? $"{wave.waveName} (Loop {_endlessLoopCount})"
            : wave.waveName;

        OnWaveStarted?.Invoke(waveIndex, name);

        _enemiesAlive = 0;
        _spawning = true;

        int totalInWave = 0;
        foreach (var group in wave.enemyGroups)
            totalInWave += Mathf.RoundToInt(group.count * countScale);

        _enemiesAlive = totalInWave;

        foreach (var group in wave.enemyGroups)
        {
            int scaledCount = Mathf.RoundToInt(group.count * countScale);
            float scaledInterval = Mathf.Max(0.15f, group.spawnInterval * intervalScale);
            StartCoroutine(SpawnGroup(group.enemyPrefab, scaledCount, scaledInterval));
        }

        // Wait until every enemy in this wave is dead
        while (_enemiesAlive > 0)
            yield return null;

        _spawning = false;
        OnWaveCompleted?.Invoke(waveIndex);
    }

    private IEnumerator SpawnGroup(GameObject prefab, int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(prefab);
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnEnemy(GameObject prefab)
    {
        float spawnZ = spawnPoint != null ? spawnPoint.position.z : fallbackSpawnZ;
        float y = spawnPoint != null ? spawnPoint.position.y : spawnY;

        // Never spawn inside the HeroCrystal's X range — clamp spawn X to horde zone
        // The crystal occupies roughly X -4 to -1.26; horde zone starts at -2.4.
        // Use the crystal's actual right edge + margin if available.
        float spawnXMin = bridgeMinX;
        float spawnXMax = bridgeMaxX;

        var crystal = HeroCrystal.Instance;
        if (crystal != null && crystal.IsPresent)
        {
            var col = crystal.GetComponent<Collider>();
            Bounds cb = col != null ? col.bounds : crystal.CrystalBounds;
            // Spawn only to the right of the crystal's right edge
            spawnXMin = Mathf.Max(spawnXMin, cb.max.x + 0.5f);
        }

        float x = UnityEngine.Random.Range(spawnXMin, spawnXMax);

        Vector3 pos = new Vector3(x, y, spawnZ);

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null && enemy.Data != null)
            enemy.Initialize(enemy.Data);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
    }
}
