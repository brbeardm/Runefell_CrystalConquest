using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Data (assign in prefab or spawner sets at runtime)")]
    [SerializeField] private EnemyData data;

    private int _currentHealth;
    private bool _isDead;
    private Action<GameObject> _releaseCallback;

    // Events other systems can listen to
    public static event Action<Enemy> OnEnemyDied;

    public EnemyData Data => data;
    public bool IsDead => _isDead;

    public void Initialize(EnemyData enemyData, Action<GameObject> releaseCallback = null)
    {
        data = enemyData;
        _releaseCallback = releaseCallback;
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

        float speed = data.moveSpeed * Time.deltaTime;

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
    }

    private void Die()
    {
        _isDead = true;
        OnEnemyDied?.Invoke(this);

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
            _currentHealth = data.maxHealth;
            transform.localScale = Vector3.one * data.scaleMultiplier;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // NOTE: Projectile damage is now handled authoritatively in PooledProjectile.OnTriggerEnter
        // via GetComponentInParent<Enemy>. This block is intentionally left as a no-op for
        // projectiles to avoid double-damage.

        // Kill shooter on contact
        if (other.CompareTag("Player"))
        {
            var health = other.GetComponentInParent<ShooterHealth>();
            if (health != null)
                health.Kill();
            Die();
        }
    }

    public void SetReleaseCallback(Action<GameObject> callback)
    {
        _releaseCallback = callback;
    }
}
