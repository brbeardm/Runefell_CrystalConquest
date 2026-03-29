using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Adaptive spawn director — monitors player performance and adjusts orc spawn pacing
/// in real-time to maintain a target tension curve.
///
/// Pressure score (0-100) is computed from:
///   - Enemies alive on bridge
///   - Kill rate (rolling window)
///   - Nearest enemy distance to player
///   - Player HP%
///   - Clone count
///   - Buff state
///
/// Each wave cycles through Build → Surge → Relief phases before triggering the boss.
/// </summary>
public class SpawnDirector : MonoBehaviour
{
    public enum WavePhase { Idle, Build, Surge, Relief, BossEntrance, BossActive, Done }

    [Header("Pressure Tuning")]
    [Tooltip("Rolling window (seconds) for kill rate computation.")]
    [SerializeField] private float killRateWindow = 8f;

    [Tooltip("How fast the smoothed pressure tracks the raw value.")]
    [SerializeField] private float pressureSmoothSpeed = 4f;

    [Header("Spawn Timing")]
    [Tooltip("Minimum seconds between spawn decisions.")]
    [SerializeField] private float minSpawnCooldown = 0.05f;

    [Tooltip("Cooldown after a burst spawn.")]
    [SerializeField] private float burstSpawnCooldown = 0.1f;

    // ── Public read-only state ───────────────────────────────────
    public float SmoothPressure { get; private set; }
    public float TargetPressure { get; private set; }
    public float RawPressure { get; private set; }
    public WavePhase CurrentPhase { get; private set; } = WavePhase.Idle;
    public int BudgetRemaining { get; private set; }
    public int EnemiesAliveCount => _aliveEnemies.Count;
    public float KillRate { get; private set; }

    public static event Action<float> OnPressureChanged;
    public static event Action<WavePhase> OnPhaseChanged;

    // ── Private state ────────────────────────────────────────────
    private WaveScalingConfig.WaveParams _wp;
    private Action<WaveScalingConfig.WaveParams> _spawnCallback;
    private Action _onBossReady;
    private int _totalBudget;
    private int _budgetSpent;

    private readonly List<Enemy> _aliveEnemies = new List<Enemy>();
    private readonly Queue<float> _killTimestamps = new Queue<float>();

    private float _spawnCooldown;
    private float _phaseTimer;
    private float _phaseDuration;
    private int _cyclesCompleted;
    private bool _bossTriggered;
    private bool _paused;

    // Player state cache
    private float _playerHPPercent = 1f;
    private int _playerMaxHP = 100;
    private int _playerCurrentHP = 100;

    // ── Lifecycle ────────────────────────────────────────────────

    private void OnEnable()
    {
        Enemy.OnEnemyDied += HandleEnemyDied;
        ShooterHealth.OnPlayerHPChanged += HandlePlayerHPChanged;
        GameManager.OnGameOver += HandleGameOver;
        ReviveManager.OnReviveStarted += HandleRevive;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
        ShooterHealth.OnPlayerHPChanged -= HandlePlayerHPChanged;
        GameManager.OnGameOver -= HandleGameOver;
        ReviveManager.OnReviveStarted -= HandleRevive;
    }

    private void Update()
    {
        if (CurrentPhase == WavePhase.Idle || CurrentPhase == WavePhase.Done || _paused)
            return;

        UpdateKillRate();
        ComputePressure();
        AdvancePhase();
        SpawnDecision();
    }

    // ── Public API ───────────────────────────────────────────────

    /// <summary>
    /// Start adaptive spawning for a wave.
    /// </summary>
    /// <param name="wp">Wave parameters from campaign config.</param>
    /// <param name="spawnOrcCallback">Called each time the director wants to spawn one orc.</param>
    /// <param name="onBossReady">Called once when pressure/budget conditions are met for boss entrance.</param>
    public void BeginWave(WaveScalingConfig.WaveParams wp, Action<WaveScalingConfig.WaveParams> spawnOrcCallback, Action onBossReady)
    {
        _wp = wp;
        _spawnCallback = spawnOrcCallback;
        _onBossReady = onBossReady;

        _totalBudget = wp.orcBudget;
        _budgetSpent = 0;
        BudgetRemaining = _totalBudget;

        _aliveEnemies.Clear();
        _killTimestamps.Clear();

        _spawnCooldown = 0f;
        _cyclesCompleted = 0;
        _bossTriggered = false;
        _paused = false;

        SmoothPressure = 0f;
        TargetPressure = wp.buildPressure.x;

        SetPhase(WavePhase.Build);
        Debug.Log($"[SpawnDirector] Wave started: budget={_totalBudget}, cycles={wp.tensionCycles}");
    }

    public void StopWave()
    {
        SetPhase(WavePhase.Idle);
        _aliveEnemies.Clear();
    }

    public bool IsWaveComplete => CurrentPhase == WavePhase.Done;

    // ── Pressure Computation ─────────────────────────────────────

    private void ComputePressure()
    {
        // Primary signal: how full is the bridge? (0-50 points)
        // 50+ enemies = full pressure from this signal
        float aliveScore = Mathf.Clamp01(_aliveEnemies.Count / 50f) * 50f;

        // Kill rate: high kill rate LOWERS pressure → spawner compensates by flooding more
        // 10+ kills/sec = player is shredding, pressure drops, spawner opens the floodgates
        float killRateScore = (1f - Mathf.Clamp01(KillRate / 10f)) * 25f;

        // Nearest enemy proximity (0-15 points)
        float nearestDist = GetNearestEnemyDistance();
        float distScore = (1f - Mathf.Clamp01(nearestDist / 40f)) * 15f;

        // Player state (0-10 points) — only ease off if player is truly struggling
        float hpScore = (1f - _playerHPPercent) * 5f;
        int cloneCount = ShooterManager.Instance != null ? ShooterManager.Instance.CloneCount : 0;
        float cloneScore = (1f - Mathf.Clamp01(cloneCount / 5f)) * 5f;

        // Buff INCREASES spawning — if player is powered up, throw more at them
        bool buffActive = CrystalBuffManager.Instance != null && CrystalBuffManager.Instance.IsBuffActive;
        float buffScore = buffActive ? -10f : 0f;

        RawPressure = Mathf.Clamp(aliveScore + killRateScore + distScore + hpScore + cloneScore + buffScore, 0f, 100f);
        SmoothPressure = Mathf.Lerp(SmoothPressure, RawPressure, pressureSmoothSpeed * Time.deltaTime);

        OnPressureChanged?.Invoke(SmoothPressure);
    }

    private float GetNearestEnemyDistance()
    {
        float playerZ = -5f;
        if (ShooterManager.Instance != null && ShooterManager.Instance.PlayerObject != null)
            playerZ = ShooterManager.Instance.PlayerObject.transform.position.z;

        float nearest = 999f;
        for (int i = _aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (_aliveEnemies[i] == null || _aliveEnemies[i].IsDead)
            {
                _aliveEnemies.RemoveAt(i);
                continue;
            }
            float dist = _aliveEnemies[i].transform.position.z - playerZ;
            if (dist < nearest) nearest = dist;
        }
        return nearest;
    }

    // ── Kill Rate ────────────────────────────────────────────────

    private void UpdateKillRate()
    {
        float cutoff = Time.time - killRateWindow;
        while (_killTimestamps.Count > 0 && _killTimestamps.Peek() < cutoff)
            _killTimestamps.Dequeue();

        KillRate = _killTimestamps.Count / killRateWindow;
    }

    // ── Phase State Machine ──────────────────────────────────────

    private void SetPhase(WavePhase phase)
    {
        CurrentPhase = phase;
        _phaseTimer = 0f;

        switch (phase)
        {
            case WavePhase.Build:
                _phaseDuration = _wp.buildDuration;
                break;
            case WavePhase.Surge:
                _phaseDuration = _wp.surgeDuration;
                TargetPressure = UnityEngine.Random.Range(_wp.surgePressure.x, _wp.surgePressure.y);
                break;
            case WavePhase.Relief:
                _phaseDuration = _wp.reliefDuration;
                TargetPressure = UnityEngine.Random.Range(_wp.reliefPressure.x, _wp.reliefPressure.y);
                break;
            case WavePhase.BossEntrance:
                TargetPressure = _wp.reliefPressure.x;
                break;
            case WavePhase.BossActive:
                TargetPressure = 45f;
                break;
        }

        Debug.Log($"[SpawnDirector] Phase → {phase}, target={TargetPressure:F0}, budget={BudgetRemaining}/{_totalBudget}");
        OnPhaseChanged?.Invoke(phase);
    }

    private void AdvancePhase()
    {
        _phaseTimer += Time.deltaTime;

        switch (CurrentPhase)
        {
            case WavePhase.Build:
                // Ramp target from buildPressure.x to buildPressure.y
                float t = Mathf.Clamp01(_phaseTimer / _phaseDuration);
                TargetPressure = Mathf.Lerp(_wp.buildPressure.x, _wp.buildPressure.y, t);
                if (_phaseTimer >= _phaseDuration)
                    SetPhase(WavePhase.Surge);
                break;

            case WavePhase.Surge:
                if (_phaseTimer >= _phaseDuration)
                    SetPhase(WavePhase.Relief);
                break;

            case WavePhase.Relief:
                if (_phaseTimer >= _phaseDuration)
                {
                    _cyclesCompleted++;
                    if (_cyclesCompleted >= _wp.tensionCycles)
                        SetPhase(WavePhase.BossEntrance);
                    else
                        SetPhase(WavePhase.Build);
                }
                break;

            case WavePhase.BossEntrance:
                // Wait for pressure to drop and budget threshold met
                float budgetFraction = (float)_budgetSpent / _totalBudget;
                if (SmoothPressure < _wp.bossEntrancePressure && budgetFraction >= _wp.bossUnlockThreshold && !_bossTriggered)
                {
                    _bossTriggered = true;
                    Debug.Log($"[SpawnDirector] Boss entrance triggered! pressure={SmoothPressure:F0}, budgetSpent={budgetFraction:P0}");
                    _onBossReady?.Invoke();
                    SetPhase(WavePhase.BossActive);
                }
                break;

            case WavePhase.BossActive:
                // Check if wave is done (budget spent + all enemies dead)
                if (BudgetRemaining <= 0 && _aliveEnemies.Count == 0)
                    SetPhase(WavePhase.Done);
                break;
        }
    }

    // ── Spawn Decision ───────────────────────────────────────────

    private void SpawnDecision()
    {
        if (BudgetRemaining <= 0) return;

        _spawnCooldown -= Time.deltaTime;
        if (_spawnCooldown > 0f) return;

        float pressureError = TargetPressure - SmoothPressure;

        // Bridge emptiness — the fewer enemies alive, the more aggressively we spawn
        float bridgeEmptiness = 1f - Mathf.Clamp01(_aliveEnemies.Count / 40f);

        if (pressureError > 15f || _aliveEnemies.Count < 5)
        {
            // Bridge is empty or big pressure deficit — flood it
            int burst = Mathf.Min(UnityEngine.Random.Range(3, 6), BudgetRemaining);
            for (int i = 0; i < burst; i++)
                SpawnOneOrc();
            _spawnCooldown = minSpawnCooldown;
        }
        else if (pressureError > 5f || _aliveEnemies.Count < 15)
        {
            // Moderate deficit or thinning bridge — keep pushing
            int count = Mathf.Min(UnityEngine.Random.Range(2, 4), BudgetRemaining);
            for (int i = 0; i < count; i++)
                SpawnOneOrc();
            _spawnCooldown = UnityEngine.Random.Range(minSpawnCooldown, burstSpawnCooldown);
        }
        else if (pressureError > -10f)
        {
            // Even if pressure is slightly above target, keep a trickle flowing
            // Only fully stop if pressure is 10+ OVER target
            SpawnOneOrc();
            _spawnCooldown = UnityEngine.Random.Range(burstSpawnCooldown, burstSpawnCooldown * 2f);
        }
        // Only stop spawning if pressure is 10+ over target (player truly overwhelmed)
    }

    private void SpawnOneOrc()
    {
        if (BudgetRemaining <= 0) return;

        _spawnCallback?.Invoke(_wp);
        _budgetSpent++;
        BudgetRemaining = _totalBudget - _budgetSpent;
    }

    // ── Enemy Tracking ───────────────────────────────────────────

    /// <summary>Call this after spawning an orc so the director can track it.</summary>
    public void TrackEnemy(Enemy enemy)
    {
        if (enemy != null && !_aliveEnemies.Contains(enemy))
            _aliveEnemies.Add(enemy);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        _aliveEnemies.Remove(enemy);
        _killTimestamps.Enqueue(Time.time);

        // Check wave completion in BossActive phase
        if (CurrentPhase == WavePhase.BossActive && BudgetRemaining <= 0 && _aliveEnemies.Count == 0)
            SetPhase(WavePhase.Done);
    }

    // ── Events ───────────────────────────────────────────────────

    private void HandlePlayerHPChanged(int current, int max)
    {
        _playerCurrentHP = current;
        _playerMaxHP = max;
        _playerHPPercent = max > 0 ? (float)current / max : 0f;
    }

    private void HandleGameOver()
    {
        _paused = true;
    }

    private void HandleRevive()
    {
        _paused = false;
    }

    public static void ResetEvents()
    {
        OnPressureChanged = null;
        OnPhaseChanged = null;
    }
}
