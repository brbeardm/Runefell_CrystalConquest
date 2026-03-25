using UnityEngine;

/// <summary>
/// Hero shooter — fires faster with high-damage projectiles.
/// Spawned when the Hero Crystal is fully broken.
/// </summary>
public class HeroShooter : AutoShooter
{
    [Header("Hero Settings")]
    [SerializeField] private int heroDamage = 100;

    protected override void OnProjectileSpawned(PooledProjectile projectile)
    {
        projectile.Damage = heroDamage;
    }
}
