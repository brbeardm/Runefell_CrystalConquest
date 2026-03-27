using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages all 8 consumable powerups — activation, timed effects, and public state queries.
/// Other scripts (ShooterHealth, PlayerShooter, PooledProjectile) check this for active effects.
/// </summary>
public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [SerializeField] private PowerupConfig config;

    // Active effect state
    private bool _shieldActive;
    private int _shieldHitsRemaining;
    private bool _ironSkinActive;
    private bool _secondWindPurchased;
    private bool _secondWindUsed;
    private bool _rapidFireActive;
    private bool _crystalFuryActive;
    private bool _armorPiercingActive;

    // Events for UI
    public static event Action<PowerupType> OnPowerupActivated;
    public static event Action<PowerupType> OnPowerupExpired;
    public static event Action<PowerupType, float> OnPowerupTimerTick; // type, remaining seconds

    // Public queries
    public bool IsShieldActive => _shieldActive && _shieldHitsRemaining > 0;
    public bool IsIronSkinActive => _ironSkinActive;
    public bool IsArmorPiercingActive => _armorPiercingActive;
    public bool HasSecondWind => _secondWindPurchased && !_secondWindUsed;

    public float DamageMultiplier
    {
        get
        {
            float mult = 1f;
            if (_crystalFuryActive)
            {
                var def = config.GetDef(PowerupType.CrystalFury);
                if (def != null) mult *= def.effectValue;
            }
            return mult;
        }
    }

    public float FireRateMultiplier
    {
        get
        {
            float mult = 1f;
            if (_rapidFireActive)
            {
                var def = config.GetDef(PowerupType.RapidFire);
                if (def != null) mult *= def.effectValue;
            }
            return mult;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        WaveSpawner.OnWaveCompleted += HandleWaveCompleted;
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        WaveSpawner.OnWaveCompleted -= HandleWaveCompleted;
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void HandleWaveCompleted(int waveIndex)
    {
        // Shield clears at end of wave
        if (_shieldActive)
        {
            _shieldActive = false;
            _shieldHitsRemaining = 0;
            OnPowerupExpired?.Invoke(PowerupType.ShieldRune);
        }
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();
        _shieldActive = false;
        _shieldHitsRemaining = 0;
        _ironSkinActive = false;
        _rapidFireActive = false;
        _crystalFuryActive = false;
        _armorPiercingActive = false;
    }

    public bool TryActivate(PowerupType type)
    {
        var def = config.GetDef(type);
        if (def == null) return false;

        // Check and spend currency
        bool canAfford = def.currencyType == CurrencyType.Gem
            ? PlayerWallet.SpendGems(def.cost)
            : PlayerWallet.SpendRunes(def.cost);
        if (!canAfford) return false;

        switch (type)
        {
            case PowerupType.HealPulse:
                foreach (var h in FindObjectsByType<ShooterHealth>(FindObjectsSortMode.None))
                {
                    if (h.IsMainPlayer)
                    {
                        h.Heal((int)def.effectValue);
                        break;
                    }
                }
                break;

            case PowerupType.ShieldRune:
                _shieldActive = true;
                _shieldHitsRemaining = (int)def.effectValue;
                break;

            case PowerupType.IronSkin:
                _ironSkinActive = true;
                StartCoroutine(RunTimedEffect(type, def.duration, () => _ironSkinActive = false));
                break;

            case PowerupType.SecondWind:
                _secondWindPurchased = true;
                break;

            case PowerupType.RapidFire:
                _rapidFireActive = true;
                StartCoroutine(RunTimedEffect(type, def.duration, () => _rapidFireActive = false));
                break;

            case PowerupType.CrystalFury:
                _crystalFuryActive = true;
                StartCoroutine(RunTimedEffect(type, def.duration, () => _crystalFuryActive = false));
                break;

            case PowerupType.ArmorPiercing:
                _armorPiercingActive = true;
                StartCoroutine(RunTimedEffect(type, def.duration, () => _armorPiercingActive = false));
                break;

            case PowerupType.CloneSurge:
                if (ShooterManager.Instance != null)
                {
                    int count = (int)def.effectValue;
                    for (int i = 0; i < count; i++)
                        ShooterManager.Instance.AddCloneShooter();
                }
                break;
        }

        OnPowerupActivated?.Invoke(type);
        return true;
    }

    /// <summary>
    /// Called by ShooterHealth when shield is active. Returns true if hit was absorbed.
    /// </summary>
    public bool TryAbsorbShieldHit()
    {
        if (!_shieldActive || _shieldHitsRemaining <= 0) return false;

        _shieldHitsRemaining--;
        if (_shieldHitsRemaining <= 0)
        {
            _shieldActive = false;
            OnPowerupExpired?.Invoke(PowerupType.ShieldRune);
        }
        return true;
    }

    /// <summary>
    /// Called by ShooterHealth when Second Wind triggers.
    /// </summary>
    public void ConsumeSecondWind()
    {
        _secondWindUsed = true;
        OnPowerupExpired?.Invoke(PowerupType.SecondWind);
    }

    private IEnumerator RunTimedEffect(PowerupType type, float duration, Action onExpire)
    {
        float remaining = duration;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            OnPowerupTimerTick?.Invoke(type, Mathf.Max(0f, remaining));
            yield return null;
        }

        onExpire?.Invoke();
        OnPowerupExpired?.Invoke(type);
    }

    public static void ResetEvents()
    {
        OnPowerupActivated = null;
        OnPowerupExpired = null;
        OnPowerupTimerTick = null;
    }
}
