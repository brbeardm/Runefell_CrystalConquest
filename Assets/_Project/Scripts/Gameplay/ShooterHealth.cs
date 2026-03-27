using UnityEngine;
using System;

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
}
