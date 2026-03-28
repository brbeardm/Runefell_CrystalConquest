using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Reusable base for animated boss behaviors with theatrical phased movement.
/// Subclasses configure distances, timings, and override hooks for boss-specific effects.
///
/// Phase flow:
///   Walk → Intimidation Attack (kill nearby orcs, roar) → Targeted Walk → Death Strike → Die
///
/// Requires an Animator with these trigger parameters:
///   "Walk", "Attack", "Die"
/// And a bool parameter:
///   "IsWalking"
///
/// ANIMATION DURATIONS (verify these match actual FBX clips):
///   - Idle: 20 frames (~0.83s at 24fps) - from Orc_TPose.fbx
///   - Walk: 47 frames (~1.96s at 24fps) - from Orc_Walking.fbx (looping disabled)
///   - Attack: ??? frames - from Orc_Attack.fbx (should match intimidationAttackTime)
///   - Die: ??? frames - from Orc_Dying.fbx (should match dyingDuration)
///
/// CONFIGURATION NOTES:
///   - All timing values are set in the prefab inspector (source of truth)
///   - OnSpawn() no longer overrides prefab values
///   - Movement is controlled by script, not root motion (applyRootMotion = false)
///   - Position is forced in LateUpdate() to prevent animation snap-back
/// </summary>
public abstract class AnimatedBossBehavior : BossBehavior
{
    public enum BossPhase { Walking, IntimidationPause, IntimidationAttack, Roar, TargetedWalk, DeathStrike, Dying, Dead }

    [Header("Animated Boss — Phases")]
    [Tooltip("Z distance to travel before intimidation attack")]
    [SerializeField] protected float intimidationTravelDistance = 5f;

    [Tooltip("Brief pause before intimidation attack (seconds)")]
    [SerializeField] protected float intimidationPauseTime = 0.5f;

    [Tooltip("How long the intimidation attack animation plays")]
    [SerializeField] protected float intimidationAttackTime = 1.5f;

    [Tooltip("Pause after attack before roar (seconds)")]
    [SerializeField] protected float roarPauseTime = 0.3f;

    [Tooltip("Roar duration before resuming walk")]
    [SerializeField] protected float roarDuration = 1.0f;

    [Header("Animated Boss — Kill Zone")]
    [Tooltip("Radius around boss to kill regular orcs during intimidation")]
    [SerializeField] protected float intimidationKillRadius = 4f;

    [Header("Animated Boss — Death Strike")]
    [Tooltip("Distance from player to stop and trigger death strike")]
    [SerializeField] protected float deathStrikeRange = 4f;

    [Tooltip("Full duration of the death strike attack animation")]
    [SerializeField] protected float deathStrikeTime = 2.0f;

    [Tooltip("Dramatic pause after the kill before showing game over screen")]
    [SerializeField] protected float deathStrikePause = 1.5f;

    [Header("Animated Boss — Death")]
    [Tooltip("Time to play dying animation before destroying")]
    [SerializeField] protected float dyingDuration = 2f;

    // State
    public BossPhase CurrentPhase { get; private set; } = BossPhase.Walking;
    protected Animator animator;
    private float _spawnZ;
    private Transform _playerTarget;
    private Vector3 _forcedPosition;

    // Events for audio/VFX hooks
    public static event Action OnBossRoar;
    public static event Action OnBossIntimidationKill;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    protected override void Start()
    {
        base.Start();

        // Take over movement from Enemy.cs and delay destruction for death animation
        if (enemy != null)
        {
            enemy.MovementOverridden = true;
            enemy.DelayDestruction = true;
        }

        _spawnZ = transform.position.z;
        _forcedPosition = transform.position;
        FindPlayer();
        SetPhase(BossPhase.Walking);
    }

    private void Update()
    {
        if (enemy == null || enemy.IsDead) return;
        if (CrystalNecromancerBossBehavior.IsResurrecting) return;

        switch (CurrentPhase)
        {
            case BossPhase.Walking:
                UpdateWalking();
                break;
            case BossPhase.TargetedWalk:
                UpdateTargetedWalk();
                break;
        }
    }

    /// <summary>
    /// Runs AFTER the Animator. Overrides any position changes the animation made.
    /// This is the fix for root motion snap-back — we own the position, not the clip.
    /// </summary>
    private void LateUpdate()
    {
        transform.position = _forcedPosition;
    }

    // ── Phase transitions ────────────────────────────────────────────────

    protected void SetPhase(BossPhase phase)
    {
        Debug.Log($"[AnimatedBoss] Phase: {CurrentPhase} → {phase}");
        CurrentPhase = phase;

        switch (phase)
        {
            case BossPhase.Walking:
                SetAnimWalking(true);
                break;

            case BossPhase.IntimidationPause:
                SetAnimWalking(false);
                StartCoroutine(RunIntimidationSequence());
                break;

            case BossPhase.TargetedWalk:
                FindPlayer();
                SetAnimWalking(true);
                break;

            case BossPhase.DeathStrike:
                SetAnimWalking(false);
                StartCoroutine(RunDeathStrike());
                break;

            case BossPhase.Dying:
                SetAnimWalking(false);
                StartCoroutine(RunDying());
                break;
        }
    }

    // ── Walking Phase ────────────────────────────────────────────────────

    private void UpdateWalking()
    {
        float speed = enemy.MoveSpeed * Time.deltaTime;
        _forcedPosition.z -= speed;

        transform.rotation = Quaternion.LookRotation(Vector3.back);

        float distanceTraveled = _spawnZ - _forcedPosition.z;
        if (distanceTraveled >= intimidationTravelDistance)
        {
            SetPhase(BossPhase.IntimidationPause);
        }
    }

    // ── Targeted Walk Phase (shooter-aware) ──────────────────────────────

    private void UpdateTargetedWalk()
    {
        FindPlayer();

        if (_playerTarget == null)
        {
            _forcedPosition.z -= enemy.MoveSpeed * Time.deltaTime;
            return;
        }

        Vector3 targetPos = _playerTarget.position;
        Vector3 dir = (targetPos - _forcedPosition).normalized;
        dir.y = 0f;

        _forcedPosition += dir * enemy.MoveSpeed * Time.deltaTime;

        // Face the player
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        // Check death strike range
        float distToPlayer = Vector3.Distance(
            new Vector3(_forcedPosition.x, 0, _forcedPosition.z),
            new Vector3(targetPos.x, 0, targetPos.z));

        if (distToPlayer <= deathStrikeRange)
        {
            SetPhase(BossPhase.DeathStrike);
        }
    }

    // ── Intimidation Sequence ────────────────────────────────────────────

    private IEnumerator RunIntimidationSequence()
    {
        CurrentPhase = BossPhase.IntimidationPause;

        // Brief menacing pause
        yield return new WaitForSeconds(intimidationPauseTime);

        // Attack animation
        CurrentPhase = BossPhase.IntimidationAttack;
        TriggerAttackAnim();

        // Wait for the impact moment (halfway through attack)
        yield return new WaitForSeconds(intimidationAttackTime * 0.5f);

        // Kill nearby regular orcs
        KillNearbyOrcs();
        OnBossIntimidationKill?.Invoke();
        OnIntimidationImpact();

        // Wait for rest of attack animation
        yield return new WaitForSeconds(intimidationAttackTime * 0.5f);

        // Roar
        CurrentPhase = BossPhase.Roar;
        yield return new WaitForSeconds(roarPauseTime);
        OnBossRoar?.Invoke();
        OnRoar();
        yield return new WaitForSeconds(roarDuration);

        // Resume walking — now targeting the player
        SetPhase(BossPhase.TargetedWalk);
    }

    // ── Death Strike ─────────────────────────────────────────────────────

    private IEnumerator RunDeathStrike()
    {
        Debug.Log("[AnimatedBoss] RunDeathStrike coroutine STARTED");

        // Suppress game over IMMEDIATELY — nothing should trigger EndGame during the cinematic
        if (ShooterManager.Instance != null)
            ShooterManager.Instance.SuppressGameOver = true;
        Debug.Log($"[AnimatedBoss] SuppressGameOver set. GameState={GameManager.Instance?.State}");

        // Switch boss animator to unscaled time so death strike plays regardless of timeScale
        if (animator != null)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

        // Make player invincible during the death strike windup so orcs don't kill them early
        if (_playerTarget != null)
        {
            var playerHealth = _playerTarget.GetComponent<ShooterHealth>();
            if (playerHealth != null)
                playerHealth.GrantInvincibility(deathStrikeTime + 5f);
        }

        TriggerAttackAnim();

        // Face the player for the killing blow
        if (_playerTarget != null)
        {
            Vector3 dir = (_playerTarget.position - transform.position).normalized;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        OnDeathStrikeStart();

        // Wait for attack animation to reach impact frame (all realtime)
        yield return new WaitForSecondsRealtime(deathStrikeTime * 0.6f);

        // Kill clones instantly, find the main player
        ShooterHealth mainPlayer = null;
        var shooters = FindObjectsByType<ShooterHealth>(FindObjectsSortMode.None);
        foreach (var health in shooters)
        {
            if (health == null) continue;

            if (health.IsMainPlayer)
            {
                mainPlayer = health;
            }
            else
            {
                health.Kill();
            }
        }

        OnDeathStrikeImpact();

        // Freeze boss in strike pose
        if (animator != null)
            animator.speed = 0f;

        // Serial flow: player death animation plays → waits → calls EndGame
        // Everything is handled inside the player's coroutine
        if (mainPlayer != null)
        {
            Debug.Log("[AnimatedBoss] Starting player death cinematic (serial)");
            yield return mainPlayer.StartCoroutine(mainPlayer.PlayDeathCinematicRoutine(6f));
        }
        else
        {
            Debug.LogWarning("[AnimatedBoss] No player found — calling EndGame directly");
            if (GameManager.Instance != null)
                GameManager.Instance.EndGame();
        }
    }

    // ── Dying ────────────────────────────────────────────────────────────

    private IEnumerator RunDying()
    {
        TriggerDieAnim();
        OnDeathStart();

        yield return new WaitForSeconds(dyingDuration);

        CurrentPhase = BossPhase.Dead;

        // Now allow destruction
        if (enemy != null)
            enemy.ForceDestroy();
    }

    /// <summary>Called by BossBehavior.OnDie() when Enemy HP reaches 0.</summary>
    public override void OnDie()
    {
        if (CurrentPhase == BossPhase.Dying || CurrentPhase == BossPhase.Dead) return;

        // Stop all running coroutines (intimidation, death strike, etc.)
        StopAllCoroutines();

        // Don't destroy immediately — play dying animation first
        // Prevent Enemy.cs from destroying us during the animation
        SetPhase(BossPhase.Dying);
    }

    // ── Kill nearby orcs ─────────────────────────────────────────────────

    private void KillNearbyOrcs()
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e == null || e.IsDead) continue;
            if (e.gameObject == gameObject) continue; // Don't kill self

            // Only kill regular orcs, not other bosses
            if (e.GetComponent<BossBehavior>() != null) continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist <= intimidationKillRadius)
                e.TakeDamage(99999);
        }
    }

    // ── Animator helpers ─────────────────────────────────────────────────

    private void SetAnimWalking(bool walking)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", walking);
            if (walking)
                animator.SetTrigger("Walk");
        }
    }

    protected void TriggerAttackAnim()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    protected void TriggerDieAnim()
    {
        if (animator != null)
            animator.SetTrigger("Die");
    }

    // ── Player targeting ─────────────────────────────────────────────────

    private void FindPlayer()
    {
        if (ShooterManager.Instance != null && ShooterManager.Instance.PlayerObject != null)
            _playerTarget = ShooterManager.Instance.PlayerObject.transform;
    }

    // ── Virtual hooks for subclass customization ─────────────────────────

    /// <summary>Called at the impact moment of the intimidation attack. Override for boss-specific VFX.</summary>
    protected virtual void OnIntimidationImpact() { }

    /// <summary>Called when the boss roars. Override for boss-specific effects (screen shake, etc).</summary>
    protected virtual void OnRoar() { }

    /// <summary>Called when death strike begins. Override for boss-specific buildup VFX.</summary>
    protected virtual void OnDeathStrikeStart() { }

    /// <summary>Called at the moment the death strike kills the player. Override for impact VFX.</summary>
    protected virtual void OnDeathStrikeImpact() { }

    /// <summary>Called when the dying animation starts. Override for boss-specific death VFX.</summary>
    protected virtual void OnDeathStart() { }

    public static void ResetEvents()
    {
        OnBossRoar = null;
        OnBossIntimidationKill = null;
    }
}
