using UnityEngine;

/// <summary>
/// Wave 5 — Crystal Orc Boss. Crystal-armored version of the Orc Boss.
/// Extra tanky (high HP via config), no special mechanic beyond visual crystal armor.
/// </summary>
public class CrystalOrcBossBehavior : BossBehavior
{
    [SerializeField] private Color crystalTint = new Color(0.3f, 0.8f, 1f, 1f);

    public override void OnSpawn()
    {
        // Apply crystal visual tint
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", crystalTint);
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", crystalTint * 0.5f);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
        Debug.Log("[Boss] Crystal Orc Boss charges in with crystal armor!");
    }
}
