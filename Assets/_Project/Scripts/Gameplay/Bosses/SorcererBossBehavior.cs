using UnityEngine;

/// <summary>
/// Wave 4 — Sorcerer Boss. Animated theatrical phases + magic shield.
/// Walk → Intimidation attack → Roar → Targeted walk → Death strike.
/// Shield absorbs first N projectile hits before taking real damage.
/// At 50% HP: Taunt → heals 500 HP, deals 25 damage to player (floor 10 HP, no kill).
/// </summary>
public class SorcererBossBehavior : AnimatedBossBehavior
{
    [Header("Sorcerer Shield")]
    [SerializeField] private int shieldHits = 15;
    [SerializeField] private Color shieldColor = new Color(0.5f, 0.2f, 1f, 0.6f);

    [Header("Sorcerer Taunt")]
    [SerializeField] private int tauntHealthBoost = 500;
    [SerializeField] private int tauntPlayerDamage = 25;
    [SerializeField] private int tauntPlayerMinHP = 10;

    private int _shieldRemaining;
    private bool _shieldActive;
    private Renderer _renderer;
    private bool _hasTaunted;

    protected override void Awake()
    {
        base.Awake();
        _renderer = GetComponentInChildren<Renderer>();
    }

    public override void OnSpawn()
    {
        intimidationTravelDistance = 5f;
        intimidationKillRadius = 5f;
        deathStrikeRange = 2.5f;

        // Sorcerer animation timings (30fps Mixamo)
        // Walking: 62 frames = 2.067s
        // Attack: 126 frames = 4.2s
        // Taunt: 85 frames = 2.833s
        // Death: 24 frames replaced → 3.633s
        intimidationAttackTime = 4.2f;
        deathStrikeTime = 4.2f;
        tauntDuration = 2.833f;
        dyingDuration = 3.633f;

        // Impact at frame 81/126 = 64.3%
        impactNormalizedTime = 0.643f;

        // Initialize shield
        _shieldRemaining = shieldHits;
        _shieldActive = true;
        UpdateShieldVisual();
        Debug.Log($"[SorcererBoss] Appears with {shieldHits}-hit shield!");
    }

    /// <summary>
    /// Called by PooledProjectile before applying damage. Returns true if shield absorbed the hit.
    /// </summary>
    public bool TryAbsorbHit()
    {
        if (!_shieldActive) return false;

        _shieldRemaining--;
        Debug.Log($"[SorcererBoss] Shield absorbed hit! {_shieldRemaining} remaining");
        if (_shieldRemaining <= 0)
        {
            _shieldActive = false;
            Debug.Log("[SorcererBoss] Shield broken!");
            CameraShake.Shake(0.4f, 0.5f);
        }
        UpdateShieldVisual();
        return true;
    }

    public bool HasShield => _shieldActive;

    protected override bool CheckTauntCondition()
    {
        if (_hasTaunted || enemy == null || enemy.IsDead) return false;

        if (enemy.CurrentHealth <= enemy.MaxHealth / 2)
        {
            _hasTaunted = true;
            Debug.Log($"[SorcererBoss] Half-health reached ({enemy.CurrentHealth}/{enemy.MaxHealth}) — TAUNT!");
            return true;
        }
        return false;
    }

    protected override void OnTauntStart()
    {
        int newHP = Mathf.Min(enemy.CurrentHealth + tauntHealthBoost, enemy.MaxHealth);
        enemy.SetHealth(newHP);
        Debug.Log($"[SorcererBoss] Taunt healed +{tauntHealthBoost}: {enemy.CurrentHealth}/{enemy.MaxHealth}");

        if (ShooterManager.Instance != null && ShooterManager.Instance.PlayerObject != null)
        {
            var playerHealth = ShooterManager.Instance.PlayerObject.GetComponent<ShooterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeTauntDamage(tauntPlayerDamage, tauntPlayerMinHP);
                Debug.Log($"[SorcererBoss] Taunt dealt {tauntPlayerDamage} damage to player (min HP: {tauntPlayerMinHP})");
            }
        }

        // Re-activate shield on taunt
        _shieldRemaining = shieldHits / 2; // Half-strength shield on taunt
        _shieldActive = true;
        UpdateShieldVisual();
        Debug.Log($"[SorcererBoss] Shield re-activated with {_shieldRemaining} hits!");
    }

    protected override void OnIntimidationImpact()
    {
        CameraShake.Shake(0.3f, 0.4f);
        StartCoroutine(ShieldPulseVFX());
    }

    protected override void OnRoar()
    {
        CameraShake.Shake(0.2f, 0.3f);
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

    private System.Collections.IEnumerator ShieldPulseVFX()
    {
        // Brief shield flash during intimidation
        if (_renderer == null || !_shieldActive) yield break;

        Color bright = shieldColor * 4f;
        Color normal = _shieldActive ? shieldColor * 2f : Color.black;

        foreach (var mat in _renderer.materials)
        {
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", bright);
        }

        yield return new WaitForSeconds(0.2f);

        foreach (var mat in _renderer.materials)
        {
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", normal);
        }
    }
}
