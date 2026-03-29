using UnityEngine;
using System;
using System.Collections;

public class ShooterHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float invincibilityDuration = 0.5f;

    private int _currentHP;
    private bool _isDead;
    private float _invincibleUntil;

    /// <summary>Fired when main player HP changes (current, max).</summary>
    public static event Action<int, int> OnPlayerHPChanged;

    public bool IsMainPlayer => GetComponent<PlayerShooter>() != null;

    public void GrantInvincibility(float duration)
    {
        _invincibleUntil = Time.time + duration;
    }

    private void Awake()
    {
        // Always start at maxHP. Clones override this in Start once ShooterManager is ready.
        _currentHP = maxHP;
    }

    private void Start()
    {
        // Clones are spawned at runtime and never have a PlayerShooter component.
        // The main player always has PlayerShooter. This avoids timing issues with ShooterManager.
        bool isMain = GetComponent<PlayerShooter>() != null;

        if (isMain)
        {
            // Keep maxHP from Awake
            OnPlayerHPChanged?.Invoke(_currentHP, maxHP);
        }
        else
        {
            // Clones have 1 HP — any touch kills them
            _currentHP = 1;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Enemy contact is now handled in Enemy.OnTriggerEnter,
        // which calls TakeDamage / Kill / BossSmash as appropriate.
    }

    /// <summary>
    /// Main player takes HP damage from regular enemies.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        // Invincibility window — prevents stacked orcs from instant-killing
        if (Time.time < _invincibleUntil) return;

        // Shield absorb check (powerup)
        if (PowerupManager.Instance != null && PowerupManager.Instance.TryAbsorbShieldHit())
        {
            _invincibleUntil = Time.time + invincibilityDuration;
            return;
        }

        // Iron Skin half damage (powerup)
        if (PowerupManager.Instance != null && PowerupManager.Instance.IsIronSkinActive)
            amount = Mathf.Max(1, amount / 2);

        _currentHP -= amount;
        _currentHP = Mathf.Max(_currentHP, 0);
        _invincibleUntil = Time.time + invincibilityDuration;

        OnPlayerHPChanged?.Invoke(_currentHP, maxHP);

        if (_currentHP <= 0)
            Kill();
    }

    /// <summary>
    /// Revive the player after cinematic death. Restores HP, re-enables all components,
    /// and resets the animator back to the Aim state.
    /// </summary>
    public void Revive()
    {
        _isDead = false;
        _currentHP = maxHP;
        OnPlayerHPChanged?.Invoke(_currentHP, maxHP);

        // Re-enable shooting
        var shooter = GetComponent<PlayerShooter>();
        if (shooter != null)
            shooter.enabled = true;

        // Re-enable collider
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        // Reset animator back to Revive state
        Animator anim = null;
        foreach (var a in GetComponentsInChildren<Animator>())
        {
            if (a.runtimeAnimatorController != null)
            {
                anim = a;
                break;
            }
        }
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.Normal;
            anim.speed = 1f;
            anim.Play("Revive", 0, 0f);
        }

        // Re-enable animation driver AFTER revive clip finishes so it doesn't freeze the animation
        StartCoroutine(ReenableAnimDriverAfterClip(anim));

        Debug.Log("[ShooterHealth] Player revived!");
    }

    private IEnumerator ReenableAnimDriverAfterClip(Animator anim)
    {
        // Wait for revive clip to finish
        if (anim != null)
        {
            yield return null; // let Play() take effect
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }

        var animDriver = GetComponent<PlayerAnimationDriver>();
        if (animDriver != null)
            animDriver.enabled = true;
    }

    /// <summary>
    /// Heal the player. Used when crystal orbs are collected at max clones.
    /// </summary>
    public void Heal(int amount)
    {
        if (_isDead) return;
        _currentHP = Mathf.Min(_currentHP + amount, maxHP);
        OnPlayerHPChanged?.Invoke(_currentHP, maxHP);
    }

    /// <summary>
    /// Boss smash — instant game over. Kills player and all clones.
    /// </summary>
    public void BossSmash()
    {
        if (_isDead) return;

        Debug.Log("[ShooterHealth] BOSS SMASH — Game Over!");

        // Kill all clones first
        if (ShooterManager.Instance != null)
            ShooterManager.Instance.KillAllShooters();
        else
            Kill();
    }

    /// <summary>
    /// Instant death (used for clones, or when player HP reaches 0).
    /// </summary>
    public void Kill()
    {
        if (_isDead) return;

        // Second Wind auto-revive (main player only)
        if (IsMainPlayer && PowerupManager.Instance != null && PowerupManager.Instance.HasSecondWind)
        {
            PowerupManager.Instance.ConsumeSecondWind();
            _currentHP = Mathf.RoundToInt(maxHP * 0.5f);
            _invincibleUntil = Time.time + 2f; // generous i-frames on revive
            OnPlayerHPChanged?.Invoke(_currentHP, maxHP);
            return;
        }

        _isDead = true;

        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RemoveShooter(gameObject);

        Destroy(gameObject);
    }

    /// <summary>
    /// Cinematic death — plays death animation, disables shooting, delays destruction.
    /// Used by boss death strike for theatrical effect.
    /// </summary>
    /// <summary>
    /// Cinematic death — plays death animation, waits for it to finish, then freezes game and calls EndGame.
    /// Everything is serial: animation plays → wait → freeze game → show game over screen.
    /// Call via StartCoroutine from the boss death strike.
    /// </summary>
    public IEnumerator PlayDeathCinematicRoutine(float animDuration)
    {
        if (_isDead) yield break;
        _isDead = true;

        _currentHP = 0;
        OnPlayerHPChanged?.Invoke(0, maxHP);

        // Disable shooting
        var shooter = GetComponent<PlayerShooter>();
        if (shooter != null)
            shooter.enabled = false;

        // Disable animation driver so it stops forcing animator.speed = 0
        var animDriver = GetComponent<PlayerAnimationDriver>();
        if (animDriver != null)
            animDriver.enabled = false;

        // Disable collider
        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Find the Animator that has a controller
        Animator anim = null;
        foreach (var a in GetComponentsInChildren<Animator>())
        {
            if (a.runtimeAnimatorController != null)
            {
                anim = a;
                break;
            }
        }

        if (anim != null)
        {
            Debug.Log($"[PlayerDeath] Animator found on '{anim.gameObject.name}', controller='{anim.runtimeAnimatorController?.name}', speed={anim.speed}, enabled={anim.enabled}, updateMode={anim.updateMode}");
            anim.enabled = true;
            anim.speed = 1f;
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.Play("Death", 0, 0f);
            Debug.Log($"[PlayerDeath] After Play('Death'): speed={anim.speed}, state='{anim.GetCurrentAnimatorStateInfo(0).shortNameHash}', normalizedTime={anim.GetCurrentAnimatorStateInfo(0).normalizedTime:F3}, length={anim.GetCurrentAnimatorStateInfo(0).length:F3}");
        }
        else
        {
            Debug.LogError("[PlayerDeath] NO Animator with controller found in children!");
        }

        // Remove from shooter manager so respawn works on revive
        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RemoveShooter(gameObject);

        // WAIT for the animation to play in REAL TIME (unaffected by timeScale)
        yield return new WaitForSecondsRealtime(0.1f);

        // Check animator state after one frame
        if (anim != null)
        {
            var stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[PlayerDeath] +0.1s check: speed={anim.speed}, normalizedTime={stateInfo.normalizedTime:F3}, length={stateInfo.length:F3}, isName('Death')={stateInfo.IsName("Death")}, playerPos={transform.position}, modelLocalPos={anim.transform.localPosition}");
        }

        yield return new WaitForSecondsRealtime(animDuration - 0.1f);

        Debug.Log("[PlayerDeath] Death animation complete, freezing game and calling EndGame");

        // NOW freeze the game and show game over / revive screen
        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (GameManager.Instance != null)
            GameManager.Instance.EndGame();
    }
}
