using System;

/// <summary>
/// Interface for ad providers. Swap implementations for Unity Ads, AdMob, etc.
/// </summary>
public interface IAdProvider
{
    bool IsRewardedAdReady();
    void ShowRewardedAd(Action onRewardGranted, Action onAdFailed);
}
