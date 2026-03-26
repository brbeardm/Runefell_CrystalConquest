using UnityEngine;

/// <summary>
/// Wave 8 — Crystal Sorcerer Boss. Has a shield like Sorcerer Boss.
/// When the shield breaks, spawns a cluster of 3-5 mini orcs at current position.
/// </summary>
public class CrystalSorcererBossBehavior : BossBehavior
{
    [SerializeField] private int shieldHits = 20;
    [SerializeField] private int miniOrcCount = 4;
    [SerializeField] private GameObject orcPrefab;
    [SerializeField] private Color shieldColor = new Color(1f, 0.3f, 0.8f, 0.6f);

    private int _shieldRemaining;
    private bool _shieldActive;
    private bool _hasSpawnedMinions;
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
        _hasSpawnedMinions = false;
        UpdateShieldVisual();
        Debug.Log($"[Boss] Crystal Sorcerer Boss appears with {shieldHits}-hit shield!");
    }

    /// <summary>
    /// Called by PooledProjectile. Returns true if shield absorbed the hit.
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (!_shieldActive) return false;

        _shieldRemaining--;
        if (_shieldRemaining <= 0)
        {
            _shieldActive = false;
            UpdateShieldVisual();
            SpawnMiniOrcs();
            Debug.Log("[Boss] Crystal Sorcerer shield shattered — mini orcs spawned!");
        }
        else
        {
            UpdateShieldVisual();
        }
        return true;
    }

    public bool HasShield => _shieldActive;

    private void SpawnMiniOrcs()
    {
        if (_hasSpawnedMinions || orcPrefab == null) return;
        _hasSpawnedMinions = true;

        for (int i = 0; i < miniOrcCount; i++)
        {
            float offsetX = Random.Range(-1f, 1f);
            float offsetZ = Random.Range(-0.5f, 0.5f);
            Vector3 pos = transform.position + new Vector3(offsetX, 0f, offsetZ);
            Instantiate(orcPrefab, pos, Quaternion.identity);
        }
    }

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
