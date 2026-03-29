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

    [Header("Debug")]
    [Tooltip("Skip to this wave index on start (0 = normal, 1 = wave 2, etc). Set to 0 for release.")]
    [SerializeField] private int debugStartWave = 0;

    [Header("Spawn Director")]
    [SerializeField] private SpawnDirector spawnDirector;

    // ── Events ──────────────────────────────────────────
    public static event Action<int, string> OnWaveStarted;      // waveIndex (0-based), waveName
    public static event Action<int> OnWaveCompleted;             // waveIndex
    public static event Action OnAllWavesCompleted;
    public static event Action<float> OnBreatherStarted;         // breather duration
    public static event Action OnVictory;
    public static event Action OnBossSpawned;

    // ── Runtime state ───────────────────────────────────
    private int _currentWaveIndex = -1;
    private int _bossesAlive;
    private bool _spawning;
    private bool _finished;
    private bool _gameOver;
    private bool _bossReadyFlag;

    public int CurrentWave => _currentWaveIndex;
    public int TotalWaves => campaignConfig != null ? campaignConfig.totalWaves : 0;
    public bool IsFinished => _finished;

    /// <summary>Total enemies alive (director orcs + bosses).</summary>
    public int EnemiesAlive => (spawnDirector != null ? spawnDirector.EnemiesAliveCount : 0) + _bossesAlive;

    private void Awake()
    {
        if (spawnDirector == null)
            spawnDirector = GetComponent<SpawnDirector>();
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDied += HandleEnemyDied;
        GameManager.OnGameStarted += HandleGameStarted;
        GameManager.OnGameOver += HandleGameOver;
        ReviveManager.OnReviveStarted += HandleRevive;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
        GameManager.OnGameStarted -= HandleGameStarted;
        GameManager.OnGameOver -= HandleGameOver;
        ReviveManager.OnReviveStarted -= HandleRevive;
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
        _gameOver = true;
    }

    private void HandleRevive()
    {
        _gameOver = false;
    }

    public void StartCampaign()
    {
        if (_spawning) return;
        if (campaignConfig == null)
        {
            Debug.LogError("[WaveSpawner] No WaveScalingConfig assigned!");
            return;
        }

        _spawning = true;
        _currentWaveIndex = -1;
        _finished = false;
        _gameOver = false;
        StartCoroutine(RunCampaign());
    }

    private IEnumerator RunCampaign()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int w = debugStartWave; w < campaignConfig.totalWaves; w++)
        {
            var wp = campaignConfig.GetWave(w);

            // Breather period
            if (wp.breatherSeconds > 0)
            {
                OnBreatherStarted?.Invoke(wp.breatherSeconds);
                yield return new WaitForSeconds(wp.breatherSeconds);
            }

            // Start wave
            _currentWaveIndex = w;
            _bossesAlive = 0;
            _bossReadyFlag = false;

            string waveName = $"Wave {w + 1}: {wp.waveName}";
            OnWaveStarted?.Invoke(w, waveName);

            // Scale crystal for this wave
            if (HeroCrystal.Instance != null)
                HeroCrystal.Instance.SetWaveScaling(wp.crystalHitsToBreak, wp.buffDuration);

            // Start adaptive spawning
            int waveIndex = w;
            spawnDirector.BeginWave(wp,
                spawnOrcCallback: (waveParams) => SpawnScaledOrc(waveParams),
                onBossReady: () =>
                {
                    _bossReadyFlag = true;
                }
            );

            // Wait for boss trigger from spawn director
            while (!_bossReadyFlag)
            {
                if (_gameOver) yield return null;
                yield return null;
            }

            // Spawn bosses
            if (waveIndex < bossPrefabs.Length && bossPrefabs[waveIndex] != null)
            {
                _bossesAlive += 2;
                Debug.Log($"[WaveSpawner] Spawning bosses for wave {w + 1}, _bossesAlive={_bossesAlive}");
                StartCoroutine(SpawnBosses(waveIndex, wp));
            }
            else
            {
                Debug.Log($"[WaveSpawner] No boss prefab for wave {w + 1}, skipping bosses");
            }

            // Wait until bosses are dead (remaining orc budget is forfeit once bosses fall)
            while (_bossesAlive > 0 || _gameOver)
                yield return null;

            Debug.Log($"[WaveSpawner] All bosses dead! Stopping director, clearing bridge. EnemiesAlive={spawnDirector.EnemiesAliveCount}");

            // Bosses down — stop spawning leftover orcs and wait for bridge to clear
            spawnDirector.StopWave();
            while (spawnDirector.EnemiesAliveCount > 0)
                yield return null;

            _spawning = false;
            Debug.Log($"[WaveSpawner] Wave {w + 1} COMPLETE!");
            OnWaveCompleted?.Invoke(w);
        }

        // All waves cleared — VICTORY!
        _finished = true;
        OnAllWavesCompleted?.Invoke();
        OnVictory?.Invoke();

        if (GameManager.Instance != null)
            GameManager.Instance.Victory();
    }

    private IEnumerator SpawnBosses(int waveIndex, WaveScalingConfig.WaveParams wp)
    {
        // Spawn Boss 1 immediately (director already waited for the right moment)
        float xPos = UnityEngine.Random.Range(bridgeMinX + 0.5f, bridgeMaxX - 0.5f);
        Enemy boss1 = SpawnBoss(waveIndex, wp, xPos);

        if (boss1 == null) yield break;

        // Wait for Boss 1 to drop below 25% HP or die, then spawn Boss 2
        bool boss2Spawned = false;
        while (!boss2Spawned)
        {
            if (boss1 == null || boss1.IsDead)
            {
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
            {
                // Randomize speed +/-20% so orcs spread out naturally like an ocean wave
                float speedVariance = wp.orcSpeed * UnityEngine.Random.Range(-0.2f, 0.2f);
                enemy.InitializeScaled(baseData, wp.orcHP, wp.orcSpeed + speedVariance);
            }

            // Register with spawn director for tracking
            if (spawnDirector != null)
                spawnDirector.TrackEnemy(enemy);
        }
    }

    private Enemy SpawnBoss(int waveIndex, WaveScalingConfig.WaveParams wp, float xPos)
    {
        if (waveIndex >= bossPrefabs.Length || bossPrefabs[waveIndex] == null) return null;

        float spawnZ = spawnPoint != null ? spawnPoint.position.z : fallbackSpawnZ;
        float y = spawnPoint != null ? spawnPoint.position.y : spawnY;

        Vector3 pos = new Vector3(xPos, y, spawnZ);
        GameObject go = Instantiate(bossPrefabs[waveIndex], pos, Quaternion.identity);

        // Set boss and all children to Enemy layer so projectiles can hit them
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        go.layer = enemyLayer;
        foreach (Transform child in go.GetComponentsInChildren<Transform>())
            child.gameObject.layer = enemyLayer;

        Enemy enemy = go.GetComponent<Enemy>();
        if (enemy != null)
        {
            var baseData = enemy.Data;
            if (baseData != null)
                enemy.InitializeScaled(baseData, wp.bossHP, wp.bossSpeed);
        }

        if (enemy != null)
            OnBossSpawned?.Invoke();

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
        // Only decrement boss count for bosses (orcs are tracked by SpawnDirector)
        if (enemy != null && enemy.GetComponent<BossBehavior>() != null)
        {
            _bossesAlive = Mathf.Max(0, _bossesAlive - 1);
            Debug.Log($"[WaveSpawner] Boss died! _bossesAlive={_bossesAlive}, enemy='{enemy.gameObject.name}'");
        }
    }
}
