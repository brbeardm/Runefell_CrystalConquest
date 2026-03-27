using System;

/// <summary>
/// Placeholder ad provider that always "shows" an ad and grants the reward immediately.
/// Replace with Unity Ads or AdMob implementation for production.
/// </summary>
public class DummyAdProvider : IAdProvider
{
    public bool IsRewardedAdReady() => true;

    public void ShowRewardedAd(Action onRewardGranted, Action onAdFailed)
    {
        // In production, this shows a real ad video.
        // For development, instantly grant the reward.
        onRewardGranted?.Invoke();
    }
}
