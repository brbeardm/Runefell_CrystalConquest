using UnityEngine;

public sealed class PooledProjectile : MonoBehaviour
{
    [Tooltip("Damage dealt on hit.")]
    public int Damage = 10;

    [Tooltip("Lifetime in seconds before auto-returning to pool.")]
    public float lifetime = 5f;

    [Tooltip("Return to pool on first collision/trigger.")]
    public bool returnOnCollision = true;

    private float _spawnTime;
    private System.Action<GameObject> _releaseCallback;

    private void OnEnable()
    {
        _spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _spawnTime >= lifetime)
            Release();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (returnOnCollision) Release();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!returnOnCollision) return;

        // ── Deal damage from the projectile side (authoritative) ──────────
        // Walk up the hierarchy from the hit collider to find an Enemy component.
        // This handles cases where the hit collider is on a child object of the enemy.
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
            enemy.TakeDamage(Damage);

        // ── Impact VFX ────────────────────────────────────────────────────
        if (ImpactVFX.Instance != null)
        {
            ImpactVFX.ImpactType type = ImpactVFX.ImpactType.Enemy;

            if (other.GetComponent<HeroCrystal>() != null || other.GetComponentInParent<HeroCrystal>() != null)
                type = ImpactVFX.ImpactType.HeroCrystal;
            else if (other.GetComponent<CrystalBall>() != null || other.GetComponentInParent<CrystalBall>() != null)
                type = ImpactVFX.ImpactType.Crystal;

            float hitSize = other.bounds.size.magnitude;
            ImpactVFX.Instance.SpawnImpact(transform.position, type, hitSize);
        }

        Release();
    }

    private void Release()
    {
        if (_releaseCallback != null)
            _releaseCallback(gameObject);
        else
            Destroy(gameObject); // fallback
    }

    public void SetReleaseCallback(System.Action<GameObject> callback)
    {
        _releaseCallback = callback;
    }
}
