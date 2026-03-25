using UnityEngine;

/// <summary>
/// Clone shooter that fires in sync with the main player.
/// Listens to PlayerShooter.OnPlayerFired instead of firing continuously.
/// </summary>
public class AutoShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Firing")]
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Pooling")]
    [SerializeField] private int poolSize = 15;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSfx;

    private SimpleObjectPool _pool;

    private void Awake()
    {
        if (firePoint == null)
            firePoint = transform;

        if (projectilePrefab != null)
            _pool = new SimpleObjectPool(projectilePrefab, poolSize, transform);
    }

    private void OnEnable()
    {
        PlayerShooter.OnPlayerFired += HandlePlayerFired;
    }

    private void OnDisable()
    {
        PlayerShooter.OnPlayerFired -= HandlePlayerFired;
    }

    private void HandlePlayerFired()
    {
        if (projectilePrefab == null) return;
        if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.GameOver)
            return;

        Fire();
    }

    protected virtual void Fire()
    {
        GameObject go = _pool != null ? _pool.Get() : Instantiate(projectilePrefab);

        go.transform.SetParent(null, true);
        go.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        go.SetActive(true);

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = firePoint.forward * projectileSpeed;

        var pooled = go.GetComponent<PooledProjectile>();
        if (pooled != null)
        {
            if (_pool != null)
                pooled.SetReleaseCallback(_pool.ReturnToPool);

            OnProjectileSpawned(pooled);
        }

        if (audioSource != null && fireSfx != null)
            audioSource.PlayOneShot(fireSfx);
    }

    /// <summary>
    /// Override to modify projectile after spawning (e.g., set damage).
    /// </summary>
    protected virtual void OnProjectileSpawned(PooledProjectile projectile) { }
}
