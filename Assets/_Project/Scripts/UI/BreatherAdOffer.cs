using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// During the breather period between waves, offers the player a chance
/// to watch a rewarded ad for +5 gems or +5 runes (alternating).
/// </summary>
public class BreatherAdOffer : MonoBehaviour
{
    [SerializeField] private GameObject offerPanel;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Text rewardText;

    private bool _rewardGems; // alternates each breather

    private void OnEnable()
    {
        WaveSpawner.OnBreatherStarted += HandleBreatherStarted;
        WaveSpawner.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        WaveSpawner.OnBreatherStarted -= HandleBreatherStarted;
        WaveSpawner.OnWaveStarted -= HandleWaveStarted;
    }

    private void Start()
    {
        if (offerPanel != null)
            offerPanel.SetActive(false);

        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(OnWatchAdClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(DismissOffer);
    }

    private void HandleBreatherStarted(float duration)
    {
        if (AdManager.Instance == null || !AdManager.Instance.IsRewardedAdReady()) return;

        _rewardGems = !_rewardGems;

        if (rewardText != null)
            rewardText.text = _rewardGems ? "Watch ad for +5 Gems?" : "Watch ad for +5 Runes?";

        if (offerPanel != null)
            offerPanel.SetActive(true);
    }

    private void HandleWaveStarted(int waveIndex, string waveName)
    {
        // Auto-dismiss when wave starts
        DismissOffer();
    }

    private void OnWatchAdClicked()
    {
        if (AdManager.Instance == null) return;

        AdManager.Instance.ShowRewardedAd(
            onReward: () =>
            {
                if (_rewardGems)
                    PlayerWallet.AddGems(5);
                else
                    PlayerWallet.AddRunes(5);
                DismissOffer();
            },
            onFail: () => DismissOffer()
        );
    }

    private void DismissOffer()
    {
        if (offerPanel != null)
            offerPanel.SetActive(false);
    }
}
