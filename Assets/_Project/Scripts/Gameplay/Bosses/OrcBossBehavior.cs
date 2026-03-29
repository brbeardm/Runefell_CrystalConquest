using UnityEngine;
using System.Collections;

/// <summary>
/// Wave 1 — Orc Boss. Giant orc with animated theatrical phases.
/// Intimidation strike creates a massive shockwave that scatters nearby orcs
/// and shakes the camera.
/// </summary>
public class OrcBossBehavior : AnimatedBossBehavior
{
    [Header("Orc Boss — Shockwave")]
    [SerializeField] private float groundSmashRadius = 6f;
    [SerializeField] private Color smashFlashColor = new Color(1f, 0.4f, 0f, 0.8f);
    [SerializeField] private float shockwaveDuration = 0.6f;

    [Header("Orc Boss — Orc Scatter")]
    [Tooltip("Radius to fling orc corpses outward.")]
    [SerializeField] private float scatterRadius = 8f;
    [Tooltip("How hard orcs are flung.")]
    [SerializeField] private float scatterForce = 15f;
    [Tooltip("Upward launch force on scattered orcs.")]
    [SerializeField] private float scatterUpForce = 6f;

    [Header("Orc Boss — Camera Shake")]
    [SerializeField] private float intimidationShakeIntensity = 0.5f;
    [SerializeField] private float intimidationShakeDuration = 0.5f;
    [SerializeField] private float deathStrikeShakeIntensity = 0.7f;
    [SerializeField] private float deathStrikeShakeDuration = 0.6f;
    [SerializeField] private float roarShakeIntensity = 0.3f;
    [SerializeField] private float roarShakeDuration = 0.4f;

    public override void OnSpawn() { }

    protected override void OnIntimidationImpact()
    {
        ScatterNearbyOrcs();
        StartCoroutine(ShockwaveVFX());
        CameraShake.Shake(intimidationShakeIntensity, intimidationShakeDuration);
    }

    protected override void OnRoar()
    {
        CameraShake.Shake(roarShakeIntensity, roarShakeDuration);
    }

    protected override void OnDeathStrikeImpact()
    {
        CameraShake.Shake(deathStrikeShakeIntensity, deathStrikeShakeDuration);
        StartCoroutine(ShockwaveVFX());
    }

    protected override void OnDeathStrikeStart()
    {
        // Boss raises weapon — brief warning shake
        CameraShake.Shake(0.15f, 0.3f);
    }

    // ── Scatter nearby orc corpses outward ───────────────────────

    private void ScatterNearbyOrcs()
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e == null || e.gameObject == gameObject) continue;
            if (e.GetComponent<BossBehavior>() != null) continue;

            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist > scatterRadius) continue;

            // Calculate launch direction (away from boss)
            Vector3 dir = (e.transform.position - transform.position).normalized;
            if (dir.sqrMagnitude < 0.001f)
                dir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

            // Distance falloff — closer orcs get flung harder
            float falloff = 1f - (dist / scatterRadius);

            // Add a Rigidbody if needed and launch
            var rb = e.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = e.gameObject.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.linearDamping = 2f;
            }
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 force = (dir + Vector3.up * scatterUpForce / scatterForce).normalized
                          * scatterForce * falloff;
            rb.AddForce(force, ForceMode.VelocityChange);

            // Add random spin for ragdoll effect
            rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.VelocityChange);
        }
    }

    // ── Shockwave VFX — expanding ring with dust layer ──────────

    private IEnumerator ShockwaveVFX()
    {
        // Outer shockwave ring
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "OrcShockwaveRing";
        ring.transform.position = transform.position + Vector3.up * 0.1f;
        var ringCol = ring.GetComponent<Collider>();
        if (ringCol != null) Destroy(ringCol);

        var ringMat = CreateTransparentMat(smashFlashColor);
        ring.GetComponent<Renderer>().material = ringMat;

        // Inner dust ring (darker, trails behind)
        var dust = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dust.name = "OrcShockwaveDust";
        dust.transform.position = transform.position + Vector3.up * 0.05f;
        var dustCol = dust.GetComponent<Collider>();
        if (dustCol != null) Destroy(dustCol);

        Color dustColor = new Color(0.6f, 0.3f, 0f, 0.5f);
        var dustMat = CreateTransparentMat(dustColor);
        dust.GetComponent<Renderer>().material = dustMat;

        float elapsed = 0f;

        while (elapsed < shockwaveDuration)
        {
            float t = elapsed / shockwaveDuration;

            // Outer ring — expands fast, thins out
            float outerScale = Mathf.Lerp(1f, groundSmashRadius * 2.5f, t);
            float outerHeight = Mathf.Lerp(0.3f, 0.05f, t);
            ring.transform.localScale = new Vector3(outerScale, outerHeight, outerScale);
            ringMat.color = new Color(smashFlashColor.r, smashFlashColor.g, smashFlashColor.b,
                smashFlashColor.a * (1f - t * t)); // Quadratic fade

            // Inner dust — expands slower, lingers longer
            float innerScale = Mathf.Lerp(0.5f, groundSmashRadius * 1.5f, t * t);
            dust.transform.localScale = new Vector3(innerScale, 0.15f, innerScale);
            dustMat.color = new Color(dustColor.r, dustColor.g, dustColor.b,
                dustColor.a * (1f - t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(ring);
        Destroy(dust);
    }

    private Material CreateTransparentMat(Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetFloat("_Surface", 1f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.color = color;
        return mat;
    }
}
