using UnityEngine;

/// <summary>
/// Wave 4 — Sorcerer Boss. Has a magic shield that absorbs the first N projectile hits.
/// After shield breaks, takes normal damage. Visual: emission toggles on shield mesh.
/// </summary>
public class SorcererBossBehavior : BossBehavior
{
    [SerializeField] private int shieldHits = 15;
    [SerializeField] private Color shieldColor = new Color(0.5f, 0.2f, 1f, 0.6f);

    private int _shieldRemaining;
    private bool _shieldActive;
    private Renderer _renderer;

    protected override void Awake()
    {
        base.Awake();
        _renderer = GetComponentInChildren<Renderer>();
    }

    public override void OnSpawn()
    {
        _shieldRemaining = shieldHits;
        _shieldActive = true;
        UpdateShieldVisual();
        Debug.Log($"[Boss] Sorcerer Boss appears with {shieldHits}-hit shield!");
    }

    public override void OnTakeDamage(int currentHP, int maxHP)
    {
        // Shield is handled via TakeDamage interception — not here.
        // OnTakeDamage only fires when actual HP damage occurs.
    }

    /// <summary>
    /// Called by PooledProjectile before applying damage. Returns true if shield absorbed the hit.
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (!_shieldActive) return false;

        _shieldRemaining--;
        if (_shieldRemaining <= 0)
        {
            _shieldActive = false;
            Debug.Log("[Boss] Sorcerer shield broken!");
        }
        UpdateShieldVisual();
        return true;
    }

    public bool HasShield => _shieldActive;

    private void UpdateShieldVisual()
    {
        if (_renderer == null) return;

        foreach (var mat in _renderer.materials)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", _shieldActive ? shieldColor * 2f : Color.black);
                mat.EnableKeyword("_EMISSION");
            }
        }
    }
}
