using UnityEngine;

/// <summary>
/// Wave 2 — Troll Boss. Animated theatrical phases + knockback on clone contact.
/// Walk 5Z → Intimidation attack (kills nearby orcs + knockback shockwave) → Roar → Targeted walk → Death strike.
/// Uses: Troll-Walking.fbx, Troll-Attack.fbx, Troll-Death.fbx
/// </summary>
public class TrollBossBehavior : AnimatedBossBehavior
{
    [Header("Troll Boss")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackRadius = 5f;
    [SerializeField] private Color stompColor = new Color(0.4f, 0.7f, 0.2f, 0.6f);

    public override void OnSpawn()
    {
        intimidationTravelDistance = 5f;
        intimidationKillRadius = 5f;
        deathStrikeRange = 2.5f;
    }

    protected override void OnIntimidationImpact()
    {
        // Ground stomp — knockback any clones in radius
        KnockbackNearbyClones();
        StartCoroutine(StompVFX());
    }

    protected override void OnRoar()
    {
        // Troll roar — could shake camera
    }

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

    // Clone knockback on direct contact (same as original behavior)
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

    private System.Collections.IEnumerator StompVFX()
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "TrollStompVFX";
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
        mat.color = stompColor;
        ring.GetComponent<Renderer>().material = mat;

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1f, knockbackRadius * 2f, t);
            ring.transform.localScale = new Vector3(scale, 0.1f, scale);
            mat.color = new Color(stompColor.r, stompColor.g, stompColor.b,
                stompColor.a * (1f - t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(ring);
    }
}
