using UnityEngine;
using System;

/// <summary>
/// Singleton wrapper for the ad provider. Swap DummyAdProvider for a real
/// implementation (Unity Ads, AdMob) when ready for production.
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private IAdProvider _provider;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _provider = new DummyAdProvider();
    }

    public bool IsRewardedAdReady() => _provider.IsRewardedAdReady();

    public void ShowRewardedAd(Action onReward, Action onFail = null)
    {
        _provider.ShowRewardedAd(onReward, onFail);
    }
}
