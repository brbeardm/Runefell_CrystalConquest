using UnityEngine;

/// <summary>
/// Wave 6 — Crystal Troll Boss. Regenerates HP over time.
/// Regen rate is set via EnemyData.healthRegenRate. Players must focus fire.
/// Also has the Troll knockback ability.
/// </summary>
public class CrystalTrollBossBehavior : BossBehavior
{
    [SerializeField] private float knockbackForce = 6f;
    [SerializeField] private Color crystalTint = new Color(0.2f, 1f, 0.5f, 1f);

    public override void OnSpawn()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", crystalTint);
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", crystalTint * 0.3f);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
        Debug.Log("[Boss] Crystal Troll Boss enters — it regenerates HP!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy != null && enemy.IsDead) return;
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponentInParent<ShooterHealth>();
        if (health == null || health.IsMainPlayer) return;

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
