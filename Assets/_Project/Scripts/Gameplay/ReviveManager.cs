using UnityEngine;
using System;

/// <summary>
/// Handles player revival after death. Three options:
/// 1. Spend gems to revive at current wave
/// 2. Watch a rewarded ad to revive for free
/// 3. Restart from wave 1
/// </summary>
public class ReviveManager : MonoBehaviour
{
    public static ReviveManager Instance { get; private set; }

    [SerializeField] private int gemReviveCost = 10;

    /// <summary>Fired when the player is revived (hides death UI, resumes game).</summary>
    public static event Action OnReviveStarted;

    public int GemReviveCost => gemReviveCost;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanReviveWithGems() => PlayerWallet.Gems >= gemReviveCost;
    public bool CanReviveWithAd() => AdManager.Instance != null && AdManager.Instance.IsRewardedAdReady();

    public void ReviveWithGems()
    {
        if (!PlayerWallet.SpendGems(gemReviveCost)) return;
        PerformRevive();
    }

    public void ReviveWithAd()
    {
        if (AdManager.Instance == null) return;
        AdManager.Instance.ShowRewardedAd(
            onReward: () => PerformRevive(),
            onFail: () => Debug.Log("[Revive] Ad failed or was dismissed.")
        );
    }

    [SerializeField] private float reviveInvincibilityDuration = 3f;

    private void PerformRevive()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RevivePlayer();

        if (ShooterManager.Instance != null)
        {
            ShooterManager.Instance.RespawnPlayer();

            // Grant extended invincibility so the player can recover
            var playerHealth = ShooterManager.Instance.PlayerObject?.GetComponent<ShooterHealth>();
            if (playerHealth != null)
                playerHealth.GrantInvincibility(reviveInvincibilityDuration);
        }

        // Crystal shockwave — expanding blast that kills nearby enemies
        var player = ShooterManager.Instance?.PlayerObject;
        if (player != null)
            ReviveBlast.Fire(player.transform.position);

        OnReviveStarted?.Invoke();
    }

    public static void ResetEvents()
    {
        OnReviveStarted = null;
    }
}
