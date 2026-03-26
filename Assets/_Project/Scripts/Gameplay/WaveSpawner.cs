using UnityEngine;
using System;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    [Header("Campaign Config")]
    [SerializeField] private WaveScalingConfig campaignConfig;

    [Header("Prefabs")]
    [Tooltip("The base orc enemy prefab (stats are overridden per wave).")]
    [SerializeField] private GameObject orcPrefab;

    [Tooltip("Boss prefabs indexed by wave (0 = wave 1 boss, 1 = wave 2 boss, etc). Assign 9.")]
    [SerializeField] private GameObject[] bossPrefabs;

    [Header("Bridge Bounds (enemy horde zone only)")]
    [SerializeField] private float bridgeMinX = -2.4f;
    [SerializeField] private float bridgeMaxX = 3.2f;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float fallbackSpawnZ = 50f;
    [SerializeField] private float spawnY = 0.5f;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private float initialDelay = 3f;

    // ── Events ──────────────────────────────────────────
    public static event Action<int, string> OnWaveStarted;      // waveIndex (0-based), waveName
    public static event Action<int> OnWaveCompleted;             // waveIndex
    public static event Action OnAllWavesCompleted;
    public static event Action<float> OnBreatherStarted;         // breather duration
    public static event Action OnVictory;

    // ── Runtime state ───────────────────────────────────
    private int _currentWaveIndex = -1;
    private int _enemiesAlive;
    private bool _spawning;
    private bool _finished;

    public int CurrentWave => _currentWaveIndex;
    public int TotalWaves => campaignConfig != null ? campaignConfig.totalWaves : 0;
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
            StartCampaign();
    }

    private void HandleGameStarted()
    {
        if (!_spawning)
            StartCampaign();
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();
        _spawning = false;
    }

    public void StartCampaign()
    {
        if (_spawning) return;
        if (campaignConfig == null)
        {
            Debug.LogError("[WaveSpawner] No WaveScalingConfig assigned!");
            return;
        }

        _currentWaveIndex = -1;
        _finished = false;
        StartCoroutine(RunCampaign());
    }

    private IEnumerator RunCampaign()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int w = 0; w < campaignConfig.totalWaves; w++)
        {
            var wp = campaignConfig.GetWave(w);

            // Breather period (no enemies, player can shoot crystal)
            if (wp.breatherSeconds > 0)
            {
                OnBreatherStarted?.Invoke(wp.breatherSeconds);
                yield return new WaitForSeconds(wp.breatherSeconds);
            }

            // Start wave
            _currentWaveIndex = w;
            _enemiesAlive = 0;
            _spawning = true;

            string waveName = $"Wave {w + 1}: {wp.waveName}";
            OnWaveStarted?.Invoke(w, waveName);

            // Scale crystal for this wave
            if (HeroCrystal.Instance != null)
                HeroCrystal.Instance.SetWaveScaling(wp.crystalHitsToBreak, wp.buffDuration);

            // Spawn orc flood
            int orcCount = wp.orcCount;
            _enemiesAlive += orcCount;
            StartCoroutine(SpawnOrcFlood(wp));

            // Spawn 2 bosses after delay
            if (w < bossPrefabs.Length && bossPrefabs[w] != null)
            {
                _enemiesAlive += 2;
                StartCoroutine(SpawnBosses(w, wp));
            }

            // Wait until every enemy in this wave is dead
            while (_enemiesAlive > 0)
                yield return null;

            _spawning = false;
            OnWaveCompleted?.Invoke(w);
        }

        // All 9 waves cleared — VICTORY!
        _finished = true;
        OnAllWavesCompleted?.Invoke();
        OnVictory?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.Victory();
    }

    private IEnumerator SpawnOrcFlood(WaveScalingConfig.WaveParams wp)
    {
        for (int i = 0; i < wp.orcCount; i++)
        {
            SpawnScaledOrc(wp);
            yield return new WaitForSeconds(wp.orcSpawnInterval);
        }
    }

    private IEnumerator SpawnBosses(int waveIndex, WaveScalingConfig.WaveParams wp)
    {
        yield return new WaitForSeconds(wp.bossSpawnDelay);

        // Spawn Boss 1
        float xPos = UnityEngine.Random.Range(bridgeMinX + 0.5f, bridgeMaxX - 0.5f);
        Enemy boss1 = SpawnBoss(waveIndex, wp, xPos);

        if (boss1 == null) yield break;

        // Wait for Boss 1 to drop below 25% HP or die, then spawn Boss 2
        bool boss2Spawned = false;
        while (!boss2Spawned)
        {
            if (boss1 == null || boss1.IsDead)
            {
                // Boss 1 died before hitting 25% — spawn Boss 2 immediately
                boss2Spawned = true;
            }
            else if (boss1.CurrentHealth <= boss1.MaxHealth * 0.25f)
            {
                boss2Spawned = true;
            }
            else
            {
                yield return null;
            }
        }

        // Spawn Boss 2 at a different X position
        float x2 = UnityEngine.Random.Range(bridgeMinX + 0.5f, bridgeMaxX - 0.5f);
        SpawnBoss(waveIndex, wp, x2);
    }

    private void SpawnScaledOrc(WaveScalingConfig.WaveParams wp)
    {
        if (orcPrefab == null) return;

        Vector3 pos = GetSpawnPosition();
        GameObject go = Instantiate(orcPrefab, pos, Quaternion.identity);

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            var baseData = enemy.Data;
            if (baseData != null)
                enemy.InitializeScaled(baseData, wp.orcHP, wp.orcSpeed);
        }
    }

    private Enemy SpawnBoss(int waveIndex, WaveScalingConfig.WaveParams wp, float xPos)
    {
        if (waveIndex >= bossPrefabs.Length || bossPrefabs[waveIndex] == null) return null;

        float spawnZ = spawnPoint != null ? spawnPoint.position.z : fallbackSpawnZ;
        float y = spawnPoint != null ? spawnPoint.position.y : spawnY;

        Vector3 pos = new Vector3(xPos, y, spawnZ);
        GameObject go = Instantiate(bossPrefabs[waveIndex], pos, Quaternion.identity);

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            var baseData = enemy.Data;
            if (baseData != null)
                enemy.InitializeScaled(baseData, wp.bossHP, wp.bossSpeed);
        }

        return enemy;
    }

    private Vector3 GetSpawnPosition()
    {
        float spawnZ = spawnPoint != null ? spawnPoint.position.z : fallbackSpawnZ;
        float y = spawnPoint != null ? spawnPoint.position.y : spawnY;

        float spawnXMin = bridgeMinX;
        float spawnXMax = bridgeMaxX;

        var crystal = HeroCrystal.Instance;
        if (crystal != null && crystal.IsPresent)
        {
            var col = crystal.GetComponent<Collider>();
            Bounds cb = col != null ? col.bounds : crystal.CrystalBounds;
            spawnXMin = Mathf.Max(spawnXMin, cb.max.x + 0.5f);
        }

        float x = UnityEngine.Random.Range(spawnXMin, spawnXMax);
        return new Vector3(x, y, spawnZ);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        _enemiesAlive = Mathf.Max(0, _enemiesAlive - 1);
    }
}
