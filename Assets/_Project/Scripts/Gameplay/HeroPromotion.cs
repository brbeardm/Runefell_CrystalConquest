using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages hero tier promotions for the player.
/// Each tier is a child GameObject containing the hero mesh and a FirePoint child.
/// Tier 0 is the base player visual (always present at start).
/// Freeing a HeroCrystal promotes the player to the next tier.
/// </summary>
public class HeroPromotion : MonoBehaviour
{
    [Serializable]
    public class HeroTier
    {
        [Tooltip("The child GameObject containing this tier's visual mesh. " +
                 "Must have a child Transform named 'FirePoint' (or assign below).")]
        public GameObject visualRoot;

        [Tooltip("Fire point for this tier. If null, will search visualRoot for a child named 'FirePoint'.")]
        public Transform firePoint;

        [Tooltip("Shots per second at this tier.")]
        public float fireRate = 6f;

        [Tooltip("Projectile damage at this tier.")]
        public int damage = 10;

        [Tooltip("How long this tier lasts (seconds). 0 = permanent (for base tier).")]
        public float duration = 0f;
    }

    [Header("Tiers (index 0 = base player, 1+ = hero tiers)")]
    [SerializeField] private HeroTier[] tiers = new HeroTier[0];

    [Header("References")]
    [SerializeField] private PlayerShooter playerShooter;

    public static event Action<int> OnTierChanged; // fires with new tier index

    public int CurrentTier => _currentTier;
    public int MaxTier => tiers.Length - 1;
    public bool IsPromoted => _currentTier > 0;

    private int _currentTier;
    private Coroutine _tierTimerCoroutine;

    private void Awake()
    {
        if (playerShooter == null)
            playerShooter = GetComponent<PlayerShooter>();

        // Auto-find fire points if not assigned
        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].firePoint == null && tiers[i].visualRoot != null)
            {
                var fp = tiers[i].visualRoot.transform.Find("FirePoint");
                if (fp != null) tiers[i].firePoint = fp;
            }
        }
    }

    private void Start()
    {
        // Ensure only tier 0 visual is active
        ApplyTier(0, skipEvents: true);
    }

    /// <summary>
    /// Promote the player to the next hero tier. Called by HeroCrystal when freed.
    /// </summary>
    public void Promote()
    {
        int nextTier = Mathf.Min(_currentTier + 1, tiers.Length - 1);
        if (nextTier == _currentTier && _currentTier > 0)
        {
            // Already at max tier — refresh the timer
            RestartTierTimer(tiers[_currentTier].duration);
            return;
        }

        ApplyTier(nextTier, skipEvents: false);
    }

    /// <summary>
    /// Demote the player back to base tier (tier 0).
    /// </summary>
    public void Demote()
    {
        if (_tierTimerCoroutine != null)
        {
            StopCoroutine(_tierTimerCoroutine);
            _tierTimerCoroutine = null;
        }

        ApplyTier(0, skipEvents: false);
    }

    /// <summary>
    /// Demote one tier (e.g., hero3 -> hero2). Falls back to base if at tier 1.
    /// </summary>
    public void DemoteOneTier()
    {
        if (_tierTimerCoroutine != null)
        {
            StopCoroutine(_tierTimerCoroutine);
            _tierTimerCoroutine = null;
        }

        int prevTier = Mathf.Max(_currentTier - 1, 0);
        ApplyTier(prevTier, skipEvents: false);
    }

    private void ApplyTier(int tierIndex, bool skipEvents)
    {
        if (tiers.Length == 0) return;
        tierIndex = Mathf.Clamp(tierIndex, 0, tiers.Length - 1);

        // Deactivate all tier visuals, then activate the target
        for (int i = 0; i < tiers.Length; i++)
        {
            if (tiers[i].visualRoot != null)
                tiers[i].visualRoot.SetActive(i == tierIndex);
        }

        _currentTier = tierIndex;
        var tier = tiers[tierIndex];

        // Update PlayerShooter stats
        if (playerShooter != null)
        {
            playerShooter.SetPromotionStats(tier.fireRate, tier.damage, tier.firePoint);
        }

        // Start tier timer if duration > 0
        if (tier.duration > 0f)
        {
            RestartTierTimer(tier.duration);
        }

        if (!skipEvents)
            OnTierChanged?.Invoke(_currentTier);
    }

    private void RestartTierTimer(float duration)
    {
        if (_tierTimerCoroutine != null)
            StopCoroutine(_tierTimerCoroutine);

        _tierTimerCoroutine = StartCoroutine(TierTimerRoutine(duration));
    }

    private IEnumerator TierTimerRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        _tierTimerCoroutine = null;

        // When timer expires, demote one tier (hero3->hero2->hero1->base)
        DemoteOneTier();
    }
}
