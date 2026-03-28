using UnityEngine;

/// <summary>
/// Wave 1 — Orc Boss. Giant orc with animated theatrical phases.
/// 
/// ORCHESTRATION:
///   1. Spawn in Idle pose (20 frames)
///   2. Walk forward 5 units at 0.5 units/sec (~10 seconds)
///   3. Intimidation Pause (0.5 seconds)
///   4. Intimidation Attack (1.5 seconds) - kills nearby orcs at 0.75s mark, ground smash VFX
///   5. Roar (0.3s pause + 1.5s roar duration)
///   6. Targeted Walk toward player
///   7. Death Strike when within 3 units of player (3 seconds) - kills player at 1.8s mark
///   8. Dying animation (3 seconds)
///
/// ANIMATIONS USED:
///   - Idle: Orc_TPose.fbx "mixamo.com (1)" (20 frames)
///   - Walk: Orc_Walking.fbx "mixamo.com" (47 frames, looping disabled)
///   - Attack: Orc_Attack.fbx "mixamo.com" (??? frames - VERIFY DURATION)
///   - Die: Orc_Dying.fbx "mixamo.com" (??? frames - VERIFY DURATION)
///
/// CONFIGURATION SOURCE:
///   All timing and distance values are set in Enemy_Boss_Orc.prefab inspector
///   (intimidationTravelDistance, intimidationKillRadius, deathStrikeRange, etc.)
/// </summary>
public class OrcBossBehavior : AnimatedBossBehavior
{
    [Header("Orc Boss")]
    [SerializeField] private float groundSmashRadius = 3f;
    [SerializeField] private Color smashFlashColor = new Color(1f, 0.5f, 0f, 0.6f);

    public override void OnSpawn()
    {
        // Orc boss defaults are now set in the prefab inspector
        // No overrides needed — use prefab values as source of truth
    }

    protected override void OnIntimidationImpact()
    {
        // Ground smash VFX — brief orange flash ring at boss feet
        StartCoroutine(GroundSmashVFX());
    }

    protected override void OnRoar()
    {
        // Could add screen shake here in future
    }

    protected override void OnDeathStrikeStart()
    {
        // Boss raises weapon — could add shadow/telegraph VFX
    }

    private System.Collections.IEnumerator GroundSmashVFX()
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "OrcSmashVFX";
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
        mat.color = smashFlashColor;
        ring.GetComponent<Renderer>().material = mat;

        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0.5f, groundSmashRadius * 2f, t);
            ring.transform.localScale = new Vector3(scale, 0.1f, scale);
            mat.color = new Color(smashFlashColor.r, smashFlashColor.g, smashFlashColor.b,
                smashFlashColor.a * (1f - t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(ring);
    }
}
