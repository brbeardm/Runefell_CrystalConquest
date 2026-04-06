using UnityEngine;

/// <summary>
/// Wave 3 — Wraith Boss. Animated theatrical phases + knockback on clone contact.
/// Walk → Intimidation attack (kills nearby orcs) → Roar → Targeted walk → Death strike.
/// At 50% HP: Taunt → heals 500 HP, deals 25 damage to player (floor 10 HP, no kill).
/// </summary>
public class WraithBossBehavior : AnimatedBossBehavior
{
    [Header("Wraith Boss")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackRadius = 5f;
    [SerializeField] private Color slashColor = new Color(0.5f, 0.1f, 0.8f, 0.6f);

    [Header("Wraith Taunt")]
    [SerializeField] private int tauntHealthBoost = 500;
    [SerializeField] private int tauntPlayerDamage = 25;
    [SerializeField] private int tauntPlayerMinHP = 10;

    private bool _hasTaunted;

    public override void OnSpawn()
    {
        intimidationTravelDistance = 5f;
        intimidationKillRadius = 5f;
        deathStrikeRange = 2.5f;

        // Wraith animation timings
        intimidationAttackTime = 2.3f;
        tauntDuration = 2.967f;
        dyingDuration = 2.967f;
        deathStrikeTime = 2.3f;

        // Impact at frame 26 of 69 = 37.7%
        impactNormalizedTime = 0.377f;
        impactFrameOffset = new Vector3(2.05f, 0.56f, -2.72f);
    }

    protected override bool CheckTauntCondition()
    {
        if (_hasTaunted || enemy == null || enemy.IsDead) return false;

        if (enemy.CurrentHealth <= enemy.MaxHealth / 2)
        {
            _hasTaunted = true;
            Debug.Log($"[WraithBoss] Half-health reached ({enemy.CurrentHealth}/{enemy.MaxHealth}) — TAUNT!");
            return true;
        }
        return false;
    }

    protected override void OnTauntStart()
    {
        int newHP = Mathf.Min(enemy.CurrentHealth + tauntHealthBoost, enemy.MaxHealth);
        enemy.SetHealth(newHP);
        Debug.Log($"[WraithBoss] Taunt healed +{tauntHealthBoost}: {enemy.CurrentHealth}/{enemy.MaxHealth}");

        if (ShooterManager.Instance != null && ShooterManager.Instance.PlayerObject != null)
        {
            var playerHealth = ShooterManager.Instance.PlayerObject.GetComponent<ShooterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeTauntDamage(tauntPlayerDamage, tauntPlayerMinHP);
                Debug.Log($"[WraithBoss] Taunt dealt {tauntPlayerDamage} damage to player (min HP: {tauntPlayerMinHP})");
            }
        }
    }

    protected override void OnIntimidationImpact()
    {
        KnockbackNearbyClones();
        StartCoroutine(SlashVFX());
    }

    protected override void OnRoar() { }

    private void KnockbackNearbyClones()
    {
        if (ShooterManager.Instance == null) return;

        var shooters = FindObjectsByType<ShooterHealth>(FindObjectsSortMode.None);
        foreach (var health in shooters)
        {
            if (health == null || health.IsMainPlayer) continue;

            float dist = Vector3.Distance(transform.position, health.transform.position);
            if (dist > knockbackRadius) continue;

            var rb = health.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockDir = (health.transform.position - transform.position).normalized;
                knockDir.y = 0.3f;
                rb.isKinematic = false;
                rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
            }
        }
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

    private System.Collections.IEnumerator SlashVFX()
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "WraithSlashVFX";
        ring.transform.position = transform.position;
        var col = ring.GetComponent<Collider>();
        if (col != null) Destroy(col);

        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetFloat("_Surface", 1f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.color = slashColor;
        ring.GetComponent<Renderer>().material = mat;

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1f, knockbackRadius * 2f, t);
            ring.transform.localScale = new Vector3(scale, 0.1f, scale);
            mat.color = new Color(slashColor.r, slashColor.g, slashColor.b,
                slashColor.a * (1f - t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(ring);
    }
}
