using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Crystal plasma shockwave with lightning arcs that expands from the player on revive.
/// Kills all enemies within range as the wave passes through them.
/// </summary>
public class ReviveBlast : MonoBehaviour
{
    [Header("Blast Settings")]
    [SerializeField] private float maxRadius = 17f;
    [SerializeField] private float expandSpeed = 20f;

    [Header("Plasma Ring")]
    [SerializeField] private Color plasmaCore = new Color(0.4f, 0.1f, 1f, 0.8f);     // deep purple
    [SerializeField] private Color plasmaEdge = new Color(0.2f, 0.8f, 1f, 0.6f);      // cyan
    [SerializeField] private Color plasmaGlow = new Color(0.7f, 0.3f, 1f, 0.4f);      // violet glow

    [Header("Lightning")]
    [SerializeField] private int lightningBoltCount = 8;
    [SerializeField] private int segmentsPerBolt = 10;
    [SerializeField] private float lightningJitter = 1.2f;
    [SerializeField] private float lightningFlickerRate = 0.05f;

    private static ReviveBlast _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public static void Fire(Vector3 origin)
    {
        if (_instance != null)
            _instance.StartCoroutine(_instance.ExpandWave(origin));
    }

    private IEnumerator ExpandWave(Vector3 origin)
    {
        var blastRoot = new GameObject("ReviveBlastVFX");
        blastRoot.transform.position = origin;

        // ── Outer glow ring ──────────────────────────────────────────────
        var outerRing = CreateRing(blastRoot.transform, "OuterGlow", plasmaGlow);

        // ── Core plasma ring ─────────────────────────────────────────────
        var coreRing = CreateRing(blastRoot.transform, "PlasmaCore", plasmaCore);

        // ── Inner bright edge ────────────────────────────────────────────
        var edgeRing = CreateRing(blastRoot.transform, "PlasmaEdge", plasmaEdge);

        // ── Lightning bolts (LineRenderers radiating outward) ────────────
        var bolts = new List<LineRenderer>();
        for (int i = 0; i < lightningBoltCount; i++)
        {
            float angle = (360f / lightningBoltCount) * i;
            var bolt = CreateLightningBolt(blastRoot.transform, angle, i);
            bolts.Add(bolt);
        }

        // ── Ground flash (brief bright disc at origin) ───────────────────
        var flash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flash.name = "GroundFlash";
        flash.transform.SetParent(blastRoot.transform, false);
        flash.transform.localPosition = Vector3.zero;
        DestroyCollider(flash);
        var flashMat = CreateTransparentMat(new Color(0.8f, 0.5f, 1f, 0.9f));
        flash.GetComponent<Renderer>().material = flashMat;
        flash.transform.localScale = new Vector3(4f, 0.05f, 4f);

        float currentRadius = 0f;
        float flickerTimer = 0f;

        while (currentRadius < maxRadius)
        {
            float dt = Time.unscaledDeltaTime;
            currentRadius += expandSpeed * dt;
            float t = currentRadius / maxRadius; // 0→1 progress

            // Scale rings at different sizes for layered plasma look
            float coreDiameter = currentRadius * 2f;
            float outerDiameter = (currentRadius + 1.5f) * 2f;
            float edgeDiameter = (currentRadius - 0.5f) * 2f;

            coreRing.transform.localScale = new Vector3(coreDiameter, 0.4f, coreDiameter);
            outerRing.transform.localScale = new Vector3(outerDiameter, 0.15f, outerDiameter);
            if (currentRadius > 0.5f)
                edgeRing.transform.localScale = new Vector3(edgeDiameter, 0.6f, edgeDiameter);

            // Fade rings as they expand
            FadeRing(coreRing, plasmaCore, t);
            FadeRing(outerRing, plasmaGlow, t * 0.8f);
            FadeRing(edgeRing, plasmaEdge, t * 0.6f);

            // Pulse the core color between purple and cyan
            float pulse = Mathf.Sin(Time.unscaledTime * 25f) * 0.5f + 0.5f;
            var pulsedColor = Color.Lerp(plasmaCore, plasmaEdge, pulse);
            pulsedColor.a = plasmaCore.a * (1f - t);
            coreRing.GetComponent<Renderer>().material.color = pulsedColor;

            // Shrink and fade ground flash
            float flashScale = Mathf.Lerp(4f, 0f, t * 2f);
            flash.transform.localScale = new Vector3(flashScale, 0.05f, flashScale);
            flashMat.color = new Color(0.8f, 0.5f, 1f, Mathf.Lerp(0.9f, 0f, t * 3f));

            // Update lightning bolts
            flickerTimer += dt;
            if (flickerTimer >= lightningFlickerRate)
            {
                flickerTimer = 0f;
                for (int i = 0; i < bolts.Count; i++)
                {
                    float angle = (360f / lightningBoltCount) * i;
                    // Drift the angle slightly each flicker for organic feel
                    angle += Random.Range(-15f, 15f);
                    UpdateLightningBolt(bolts[i], origin, currentRadius, angle, t);
                }
            }

            // Kill enemies within the expanding radius
            KillEnemiesInRadius(origin, currentRadius);

            yield return null;
        }

        // Brief linger then destroy
        yield return new WaitForSecondsRealtime(0.15f);
        Destroy(blastRoot);
    }

    // ── Ring creation ────────────────────────────────────────────────────

    private GameObject CreateRing(Transform parent, string name, Color color)
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = name;
        ring.transform.SetParent(parent, false);
        ring.transform.localPosition = Vector3.zero;
        DestroyCollider(ring);
        ring.GetComponent<Renderer>().material = CreateTransparentMat(color);
        ring.transform.localScale = Vector3.zero;
        return ring;
    }

    private void FadeRing(GameObject ring, Color baseColor, float t)
    {
        var mat = ring.GetComponent<Renderer>().material;
        mat.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
    }

    // ── Lightning bolt creation & update ─────────────────────────────────

    private LineRenderer CreateLightningBolt(Transform parent, float angle, int index)
    {
        var boltGO = new GameObject($"Lightning_{index}");
        boltGO.transform.SetParent(parent, false);

        var lr = boltGO.AddComponent<LineRenderer>();
        lr.positionCount = segmentsPerBolt;
        lr.startWidth = 0.15f;
        lr.endWidth = 0.05f;
        lr.useWorldSpace = true;
        lr.numCapVertices = 2;

        // Lightning material — bright emissive white-cyan
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetFloat("_Surface", 1f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // additive
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3100;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.color = new Color(0.7f, 0.85f, 1f, 0.9f);
        lr.material = mat;

        // Gradient: white core → cyan → purple tips
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.3f, 0.8f, 1f), 0.3f),
                new GradientColorKey(new Color(0.6f, 0.2f, 1f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        lr.colorGradient = gradient;

        return lr;
    }

    private void UpdateLightningBolt(LineRenderer lr, Vector3 origin, float radius, float angle, float t)
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

        for (int i = 0; i < segmentsPerBolt; i++)
        {
            float segT = (float)i / (segmentsPerBolt - 1);
            Vector3 basePos = origin + dir * (radius * segT);

            // Add jagged jitter perpendicular to the bolt direction
            if (i > 0 && i < segmentsPerBolt - 1)
            {
                Vector3 perp = Vector3.Cross(dir, Vector3.up);
                float jitter = Random.Range(-lightningJitter, lightningJitter) * (1f - t);
                float vertJitter = Random.Range(-lightningJitter * 0.5f, lightningJitter * 0.5f) * (1f - t);
                basePos += perp * jitter + Vector3.up * vertJitter;
            }

            // Lift slightly off ground
            basePos.y = origin.y + 0.5f + Random.Range(0f, 0.3f);
            lr.SetPosition(i, basePos);
        }

        // Fade out bolts as blast expands
        float alpha = Mathf.Lerp(0.9f, 0f, t);
        lr.startWidth = Mathf.Lerp(0.2f, 0.05f, t);
        lr.endWidth = Mathf.Lerp(0.1f, 0.02f, t);

        var mat = lr.material;
        var c = mat.color;
        mat.color = new Color(c.r, c.g, c.b, alpha);
    }

    // ── Enemy kill zone ──────────────────────────────────────────────────

    private void KillEnemiesInRadius(Vector3 origin, float radius)
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(origin, enemy.transform.position);
            if (dist <= radius)
                enemy.TakeDamage(99999);
        }
    }

    // ── Utilities ────────────────────────────────────────────────────────

    private Material CreateTransparentMat(Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
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

    private void DestroyCollider(GameObject go)
    {
        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
}
