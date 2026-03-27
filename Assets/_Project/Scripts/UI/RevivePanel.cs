using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Death screen with 3 revive options:
/// 1. Spend gems to revive at current wave
/// 2. Watch rewarded ad to revive for free
/// 3. Restart from wave 1
/// </summary>
public class RevivePanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Gem Revive")]
    [SerializeField] private Button gemReviveButton;
    [SerializeField] private Text gemReviveCostText;

    [Header("Ad Revive")]
    [SerializeField] private Button adReviveButton;

    [Header("Restart")]
    [SerializeField] private Button restartButton;

    [Header("Info")]
    [SerializeField] private Text scoreText;

    private void OnEnable()
    {
        GameManager.OnGameOver += ShowPanel;
        ReviveManager.OnReviveStarted += HidePanel;
        Debug.Log("[RevivePanel] Subscribed to OnGameOver");
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= ShowPanel;
        ReviveManager.OnReviveStarted -= HidePanel;
    }

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (gemReviveButton != null)
            gemReviveButton.onClick.AddListener(OnGemReviveClicked);

        if (adReviveButton != null)
            adReviveButton.onClick.AddListener(OnAdReviveClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    private void ShowPanel()
    {
        Debug.Log($"[RevivePanel] ShowPanel called! panel={panel}, panel is null={panel == null}");
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log($"[RevivePanel] Panel activated: {panel.activeSelf}");
        }
        else
        {
            Debug.LogError("[RevivePanel] Panel reference is NULL — not assigned in Inspector!");
        }

        // Update gem revive button
        if (ReviveManager.Instance != null)
        {
            bool canAfford = ReviveManager.Instance.CanReviveWithGems();
            if (gemReviveButton != null)
                gemReviveButton.interactable = canAfford;
            if (gemReviveCostText != null)
                gemReviveCostText.text = $"Revive ({ReviveManager.Instance.GemReviveCost} Gems)";
        }

        // Update ad revive button
        if (adReviveButton != null)
        {
            bool adReady = ReviveManager.Instance != null && ReviveManager.Instance.CanReviveWithAd();
            adReviveButton.interactable = adReady;
        }

        // Show score
        if (scoreText != null && GameManager.Instance != null)
            scoreText.text = $"Score: {GameManager.Instance.Score}";
    }

    private void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnGemReviveClicked()
    {
        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ReviveWithGems();
    }

    private void OnAdReviveClicked()
    {
        if (ReviveManager.Instance != null)
            ReviveManager.Instance.ReviveWithAd();
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}
