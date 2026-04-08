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
    public enum BossPhase { Walking, IntimidationPause, IntimidationAttack, Roar, TargetedWalk, Taunting, DeathStrike, Dying, Dead }

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

    [Tooltip("Ideal distance from player for the weapon to connect during death strike")]
    [SerializeField] protected float deathStrikeImpactDistance = 1.5f;

    [Tooltip("Time (seconds) for boss to slide into strike position during windup")]
    [SerializeField] protected float deathStrikeSlideTime = 0.4f;

    [Tooltip("Child GameObject on the weapon marking where it hits (e.g. mace tip). Currently used for reference only.")]
    [SerializeField] protected GameObject weaponImpactPoint;

    [Tooltip("Position tuning for the death strike. Applied in the boss's local space at impact. X = left/right shift, Z = forward(+)/backward(-) shift relative to deathStrikeImpactDistance.")]
    [SerializeField] protected Vector3 impactPositionTuning = Vector3.zero;

    [Tooltip("Normalized time (0-1) in the attack animation when the weapon makes contact. Used to time the death strike slide.")]
    [SerializeField] protected float impactNormalizedTime = 0.58f;

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
    public static event Action OnBossTaunt;
    public static event Action OnBossTauntEnd;

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

    private float _lastDebugTime;

    private void Update()
    {
        if (enemy == null) return;
        if (enemy.IsDead)
        {
            if (Time.time - _lastDebugTime > 2f)
            {
                Debug.Log($"[AnimatedBoss] Update skipped — IsDead=true, phase={CurrentPhase}, HP={enemy.CurrentHealth}/{enemy.MaxHealth}");
                _lastDebugTime = Time.time;
            }
            return;
        }
        if (CrystalNecromancerBossBehavior.IsResurrecting) return;

        switch (CurrentPhase)
        {
            case BossPhase.Walking:
                UpdateWalking();
                break;
            
            case BossPhase.IntimidationPause:
            case BossPhase.IntimidationAttack:
            case BossPhase.Roar:
                // Continue moving forward during intimidation sequence
                _forcedPosition.z -= enemy.MoveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(Vector3.back);
                break;
            
            case BossPhase.TargetedWalk:
                if (CheckTauntCondition())
                {
                    SetPhase(BossPhase.Taunting);
                    break;
                }
                UpdateTargetedWalk();
                break;
            
            case BossPhase.Taunting:
                // Boss stands still during taunt
                break;
            
            case BossPhase.DeathStrike:
                // Boss keeps walking forward (off bridge) after killing the player
                _forcedPosition.z -= enemy.MoveSpeed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(Vector3.back);
                // Destroy boss once it walks well past the player and off-screen
                if (_forcedPosition.z < -15f)
                {
                    Debug.Log("[AnimatedBoss] Boss walked off bridge — destroying");
                    if (!enemy.IsDead)
                        enemy.TakeDamage(99999); // triggers Die() → OnEnemyDied → decrements _enemiesAlive
                    enemy.ForceDestroy();
                }
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
                // Keep walk animation playing until attack trigger fires — avoids Idle dip
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

            case BossPhase.Taunting:
                SetAnimWalking(false);
                StartCoroutine(RunTaunt());
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

        // As boss approaches death strike range, steer toward bridge center X
        // so the attack animation has room to play without going off the edge
        float distToPlayer = Vector3.Distance(
            new Vector3(_forcedPosition.x, 0, _forcedPosition.z),
            new Vector3(targetPos.x, 0, targetPos.z));

        float steerRange = deathStrikeRange * 3f; // start steering at 3x death strike range
        if (distToPlayer < steerRange)
        {
            float steerT = 1f - (distToPlayer / steerRange); // 0 at far, 1 at close
            float bridgeCenterX = 0f;
            targetPos.x = Mathf.Lerp(targetPos.x, bridgeCenterX, steerT);
        }

        Vector3 dir = (targetPos - _forcedPosition).normalized;
        dir.y = 0f;

        _forcedPosition += dir * enemy.MoveSpeed * Time.deltaTime;

        // Smoothly rotate toward the player (no snapping)
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }

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

        // Resume walking — smooth crossfade from attack/roar into walk
        if (animator != null)
            animator.ResetTrigger("Attack");

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

        // Freeze the player — disable movement and shooting so they can't escape
        PlayerMover playerMover = null;
        PlayerShooter playerShooter = null;
        PlayerAnimationDriver playerAnimDriver = null;
        if (_playerTarget != null)
        {
            playerMover = _playerTarget.GetComponent<PlayerMover>();
            playerShooter = _playerTarget.GetComponent<PlayerShooter>();
            playerAnimDriver = _playerTarget.GetComponent<PlayerAnimationDriver>();

            if (playerMover != null) playerMover.enabled = false;
            if (playerShooter != null) playerShooter.enabled = false;

            var playerHealth = _playerTarget.GetComponent<ShooterHealth>();
            if (playerHealth != null)
                playerHealth.GrantInvincibility(deathStrikeTime + 5f);

            Debug.Log("[AnimatedBoss] Player frozen for death strike");
        }

        // Lock facing — boss is already oriented toward the player from TargetedWalk.
        // No rotation changes after this point so the attack animation plays straight.

        TriggerAttackAnim();
        OnDeathStrikeStart();

        // Slide boss straight forward to deathStrikeImpactDistance from the player.
        // Single smooth motion — no dynamic weapon tracking, no rotation changes.
        Vector3 slideStart = _forcedPosition;
        Vector3 slideTarget = slideStart;
        if (_playerTarget != null)
        {
            Vector3 toPlayer = _playerTarget.position - _forcedPosition;
            toPlayer.y = 0f;
            Vector3 dirToPlayer = toPlayer.normalized;
            slideTarget = _playerTarget.position - dirToPlayer * deathStrikeImpactDistance;
            // Apply per-boss tuning in the boss's local space (X = left/right, Z = forward/back)
            slideTarget += transform.rotation * impactPositionTuning;
            slideTarget.y = slideStart.y;
        }

        Debug.Log($"[AnimatedBoss] DeathStrike slide: start={slideStart}, target={slideTarget}, " +
                  $"playerPos={_playerTarget?.position}, impactDist={deathStrikeImpactDistance}, tuning={impactPositionTuning}");

        float impactTime = deathStrikeTime * impactNormalizedTime;
        float slideElapsed = 0f;
        while (slideElapsed < impactTime)
        {
            slideElapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, slideElapsed / impactTime);
            _forcedPosition = Vector3.Lerp(slideStart, slideTarget, t);
            yield return null;
        }
        _forcedPosition = slideTarget;

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

        // Boss walks off in victory — resume walking animation
        if (animator != null)
        {
            animator.speed = 1f;
            SetAnimWalking(true);
            Debug.Log("[AnimatedBoss] Boss resuming walk (victory stride off bridge)");
        }

        // Start the player death cinematic (non-blocking so boss keeps moving)
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

    // ── Taunt ────────────────────────────────────────────────────────────

    [Header("Animated Boss — Taunt")]
    [Tooltip("Duration of the taunt animation")]
    [SerializeField] protected float tauntDuration = 2.5f;

    private IEnumerator RunTaunt()
    {
        // Wait one frame so the Walk→Idle transition settles before firing the trigger
        yield return null;

        TriggerTauntAnim();
        OnBossTaunt?.Invoke();
        OnTauntStart();

        // Sustained camera shake for the full taunt duration (no decay)
        CameraShake.Shake(0.3f, tauntDuration, sustained: true);

        yield return new WaitForSeconds(tauntDuration);

        OnTauntEnd();
        OnBossTauntEnd?.Invoke();

        // Resume walking — smooth crossfade from taunt into walk
        if (animator != null)
            animator.ResetTrigger("Taunt");

        SetPhase(BossPhase.TargetedWalk);
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
                animator.CrossFade("Walk", 0.2f, 0);
        }
    }

    protected void TriggerAttackAnim()
    {
        if (animator != null)
            animator.SetTrigger("Attack");
    }

    protected void TriggerTauntAnim()
    {
        if (animator != null)
        {
            Debug.Log($"[AnimatedBoss] TriggerTauntAnim — animator enabled={animator.enabled}, current state={animator.GetCurrentAnimatorStateInfo(0).shortNameHash}, IsInTransition={animator.IsInTransition(0)}");
            animator.SetTrigger("Taunt");

            // Verify parameter exists
            foreach (var param in animator.parameters)
            {
                if (param.name == "Taunt")
                {
                    Debug.Log($"[AnimatedBoss] 'Taunt' parameter found, type={param.type}");
                }
            }
        }
        else
        {
            Debug.LogError("[AnimatedBoss] TriggerTauntAnim — animator is NULL!");
        }
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

    /// <summary>Called each frame during TargetedWalk. Return true to trigger taunt phase. Default: no taunt.</summary>
    protected virtual bool CheckTauntCondition() => false;

    /// <summary>Called when the taunt animation starts. Override for boss-specific taunt effects (heal, debuff, etc).</summary>
    protected virtual void OnTauntStart() { }

    /// <summary>Called when the taunt animation ends. Override for post-taunt effects.</summary>
    protected virtual void OnTauntEnd() { }

    /// <summary>Called when the dying animation starts. Override for boss-specific death VFX.</summary>
    protected virtual void OnDeathStart() { }

    public static void ResetEvents()
    {
        OnBossRoar = null;
        OnBossIntimidationKill = null;
        OnBossTaunt = null;
        OnBossTauntEnd = null;
    }
}
