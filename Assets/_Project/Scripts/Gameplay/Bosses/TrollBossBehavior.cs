using UnityEngine;

/// <summary>
/// Wave 2 — Troll Boss. On contact with a clone, knocks it back before killing it.
/// The knockback is visual — the clone gets pushed, then destroyed.
/// </summary>
public class TrollBossBehavior : BossBehavior
{
    [SerializeField] private float knockbackForce = 5f;

    public override void OnSpawn()
    {
        Debug.Log("[Boss] Troll Boss stomps onto the bridge!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy != null && enemy.IsDead) return;
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponentInParent<ShooterHealth>();
        if (health == null || health.IsMainPlayer) return;

        // Knockback the clone before it dies
        var rb = other.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            Vector3 knockDir = (other.transform.position - transform.position).normalized;
            knockDir.y = 0.3f;
            rb.isKinematic = false;
            rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
        }
    }
}
