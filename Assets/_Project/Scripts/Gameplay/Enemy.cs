using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Data (assign in prefab or spawner sets at runtime)")]
    [SerializeField] private EnemyData data;

    private int _currentHealth;
    private int _maxHealth;
    private bool _isDead;
    private Action<GameObject> _releaseCallback;

    // Scaled overrides (set by InitializeScaled, -1 means use data defaults)
    private int _overrideHP = -1;
    private float _overrideSpeed = -1f;

    // Events
    public static event Action<Enemy> OnEnemyDied;
    public event Action<int, int> OnDamageTaken; // currentHP, maxHP

    public EnemyData Data => data;
    public bool IsDead => _isDead;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public float MoveSpeed => _overrideSpeed > 0 ? _overrideSpeed : (data != null ? data.moveSpeed : 0f);

    /// <summary>When true, Enemy.Update() skips default movement. BossBehavior controls position directly.</summary>
    public bool MovementOverridden { get; set; }

    public void Initialize(EnemyData enemyData, Action<GameObject> releaseCallback = null)
    {
        data = enemyData;
        _releaseCallback = releaseCallback;
        _overrideHP = -1;
        _overrideSpeed = -1f;
        ResetEnemy();
    }

    /// <summary>
    /// Initialize with per-wave scaled stats (overrides EnemyData defaults).
    /// </summary>
    public void InitializeScaled(EnemyData enemyData, int overrideHP, float overrideSpeed, Action<GameObject> releaseCallback = null)
    {
        data = enemyData;
        _releaseCallback = releaseCallback;
        _overrideHP = overrideHP;
        _overrideSpeed = overrideSpeed;
        ResetEnemy();
    }

    private void OnEnable()
    {
        ResetEnemy();
    }

    private const float SidestepSpeed = 3f; // How fast enemy slides right past the crystal

    private void Update()
    {
        if (_isDead || data == null) return;

        // Freeze during Necromancer resurrection
        if (CrystalNecromancerBossBehavior.IsResurrecting) return;

        // Boss behavior is controlling movement directly
        if (MovementOverridden) return;

        // HP regen (used by Crystal Troll boss)
        if (data.healthRegenRate > 0 && _currentHealth < _maxHealth)
        {
            _currentHealth = Mathf.Min(_currentHealth + Mathf.CeilToInt(data.healthRegenRate * Time.deltaTime), _maxHealth);
            OnDamageTaken?.Invoke(_currentHealth, _maxHealth);
        }

        float speed = MoveSpeed * Time.deltaTime;

        // Get crystal wall X — enemies must stay to the RIGHT of this
        float crystalWallX = BridgeZoneConstants.EnemyHordeMinX; // default: left bridge edge
        float crystalFrontZ = float.MinValue;
        float crystalBackZ  = float.MaxValue;

        var crystal = HeroCrystal.Instance;
        if (crystal != null && crystal.IsPresent)
        {
            var col = crystal.GetComponent<Collider>();
            Bounds cb = col != null ? col.bounds : crystal.CrystalBounds;
            crystalWallX  = cb.max.x + 0.3f;  // right edge of crystal + margin
            crystalFrontZ = cb.max.z;           // front face (enemies approach from +Z)
            crystalBackZ  = cb.min.z;           // back face
        }

        Vector3 pos = transform.position;

        // Is this enemy in the Z range of the crystal?
        bool blockedByWall = pos.x < crystalWallX
                          && pos.z < crystalFrontZ + 0.5f
                          && pos.z > crystalBackZ  - 0.5f;

        if (blockedByWall)
        {
            // Hit the wall — stop forward movement, sidestep RIGHT only
            pos.x += SidestepSpeed * Time.deltaTime;
        }
        else
        {
            // Clear of crystal — move straight forward
            pos.z -= speed;
        }

        // Hard clamp — never allowed left of crystal wall, never off bridge
        pos.x = Mathf.Clamp(pos.x, crystalWallX, BridgeZoneConstants.EnemyHordeMaxX);

        transform.position = pos;

        // Destroy enemies that walk off the bridge past the player
        if (pos.z < -15f)
            Die();
    }

    public void TakeDamage(int amount)
    {
        if (_isDead) return;

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            Die();
        }
        else
        {
            OnDamageTaken?.Invoke(_currentHealth, _maxHealth);
        }
    }

    /// <summary>When true, Die() skips immediate destruction. Boss calls ForceDestroy() after death animation.</summary>
    public bool DelayDestruction { get; set; }

    private void Die()
    {
        _isDead = true;

        // Notify boss behavior before destruction
        var boss = GetComponent<BossBehavior>();
        if (boss != null)
            boss.OnDie();

        OnEnemyDied?.Invoke(this);

        // Animated bosses delay destruction for the dying animation
        if (DelayDestruction) return;

        if (_releaseCallback != null)
            _releaseCallback(gameObject);
        else
            Destroy(gameObject);
    }

    /// <summary>Force destroy after delayed death animation completes.</summary>
    public void ForceDestroy()
    {
        if (_releaseCallback != null)
            _releaseCallback(gameObject);
        else
            Destroy(gameObject);
    }

    private void ResetEnemy()
    {
        _isDead = false;
        if (data != null)
        {
            _maxHealth = _overrideHP > 0 ? _overrideHP : data.maxHealth;
            _currentHealth = _maxHealth;
            transform.localScale = Vector3.one * data.scaleMultiplier;
        }
    }

    /// <summary>
    /// Directly set health (used by Necromancer heal).
    /// </summary>
    public void SetHealth(int hp)
    {
        _currentHealth = Mathf.Clamp(hp, 0, _maxHealth);
        OnDamageTaken?.Invoke(_currentHealth, _maxHealth);
    }

    private void OnTriggerEnter(Collider other)
    {
        // NOTE: Projectile damage is handled authoritatively in PooledProjectile.OnTriggerEnter.

        // Animated bosses handle player contact via their death strike coroutine
        if (MovementOverridden && data != null && data.isInstantKill) return;

        // Contact with shooter
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponentInParent<ShooterHealth>();
            if (health == null) return;

            if (data != null && data.isInstantKill)
            {
                // Boss smash = instant game over
                health.BossSmash();
            }
            else if (health.IsMainPlayer)
            {
                // Main player takes HP damage from orcs
                health.TakeDamage(data != null ? data.contactDamage : 10);
            }
            else
            {
                // Clone dies on any enemy contact
                health.Kill();
            }

            Die();
        }
    }

    public void SetReleaseCallback(Action<GameObject> callback)
    {
        _releaseCallback = callback;
    }
}
