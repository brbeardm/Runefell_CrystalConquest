using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bottom bar HUD for the 8 consumable powerups + gem/rune currency display.
/// Layout: [Gems] [Heal][Shield][Iron][2ndWind] [Crystal Bar] [Rapid][Fury][Pierce][Clone] [Runes]
/// </summary>
public class PowerupHUD : MonoBehaviour
{
    [SerializeField] private PowerupConfig config;

    [Header("Currency Displays")]
    [SerializeField] private Text gemCountText;
    [SerializeField] private Text runeCountText;

    [Header("Powerup Buttons (order: HealPulse, ShieldRune, IronSkin, SecondWind, RapidFire, CrystalFury, ArmorPiercing, CloneSurge)")]
    [SerializeField] private Button[] powerupButtons = new Button[8];
    [SerializeField] private Text[] powerupCostTexts = new Text[8];
    [SerializeField] private Sprite[] powerupIcons = new Sprite[8];

    [Header("Timer Overlays (filled Image for countdown, same order as buttons)")]
    [SerializeField] private Image[] timerOverlays = new Image[8];

    // Track durations for timer fill
    private float[] _maxDurations = new float[8];

    private void OnEnable()
    {
        PlayerWallet.OnCurrencyChanged += UpdateCurrencyDisplay;
        PowerupManager.OnPowerupActivated += HandlePowerupActivated;
        PowerupManager.OnPowerupExpired += HandlePowerupExpired;
        PowerupManager.OnPowerupTimerTick += HandleTimerTick;
    }

    private void OnDisable()
    {
        PlayerWallet.OnCurrencyChanged -= UpdateCurrencyDisplay;
        PowerupManager.OnPowerupActivated -= HandlePowerupActivated;
        PowerupManager.OnPowerupExpired -= HandlePowerupExpired;
        PowerupManager.OnPowerupTimerTick -= HandleTimerTick;
    }

    private void Start()
    {
        // Wire button clicks
        for (int i = 0; i < powerupButtons.Length; i++)
        {
            if (powerupButtons[i] == null) continue;
            int index = i;
            PowerupType type = (PowerupType)index;
            powerupButtons[i].onClick.AddListener(() => OnPowerupClicked(type));
        }

        // Set up icons and cost text from config
        if (config != null)
        {
            for (int i = 0; i < 8; i++)
            {
                var def = config.GetDef((PowerupType)i);
                if (def == null) continue;

                // Apply icon sprite to the button's Image component
                if (powerupIcons[i] != null && powerupButtons[i] != null)
                {
                    var btnImage = powerupButtons[i].GetComponent<Image>();
                    if (btnImage != null)
                        btnImage.sprite = powerupIcons[i];
                }
                else if (def.icon != null && powerupButtons[i] != null)
                {
                    var btnImage = powerupButtons[i].GetComponent<Image>();
                    if (btnImage != null)
                        btnImage.sprite = def.icon;
                }

                if (powerupCostTexts[i] != null)
                    powerupCostTexts[i].text = def.cost.ToString();

                _maxDurations[i] = def.duration;
            }
        }

        // Hide all timer overlays
        for (int i = 0; i < timerOverlays.Length; i++)
        {
            if (timerOverlays[i] != null)
            {
                timerOverlays[i].fillAmount = 0f;
                timerOverlays[i].gameObject.SetActive(false);
            }
        }

        // Disable raycastTarget on non-button elements so they don't
        // steal input from shooting when the player hovers over the HUD
        DisableNonButtonRaycasts();

        UpdateCurrencyDisplay(PlayerWallet.Gems, PlayerWallet.Runes);
    }

    private void DisableNonButtonRaycasts()
    {
        // Currency text
        if (gemCountText != null) gemCountText.raycastTarget = false;
        if (runeCountText != null) runeCountText.raycastTarget = false;

        // Cost texts and timer overlays
        for (int i = 0; i < 8; i++)
        {
            if (i < powerupCostTexts.Length && powerupCostTexts[i] != null)
                powerupCostTexts[i].raycastTarget = false;

            if (i < timerOverlays.Length && timerOverlays[i] != null)
                timerOverlays[i].raycastTarget = false;
        }
    }

    private void OnPowerupClicked(PowerupType type)
    {
        if (PowerupManager.Instance != null)
            PowerupManager.Instance.TryActivate(type);
        RefreshAffordability();
    }

    private void UpdateCurrencyDisplay(int gems, int runes)
    {
        if (gemCountText != null) gemCountText.text = gems.ToString();
        if (runeCountText != null) runeCountText.text = runes.ToString();
        RefreshAffordability();
    }

    private void RefreshAffordability()
    {
        if (config == null) return;

        for (int i = 0; i < powerupButtons.Length; i++)
        {
            if (powerupButtons[i] == null) continue;

            var def = config.GetDef((PowerupType)i);
            if (def == null) continue;

            bool canAfford = def.currencyType == CurrencyType.Gem
                ? PlayerWallet.Gems >= def.cost
                : PlayerWallet.Runes >= def.cost;

            powerupButtons[i].interactable = canAfford;

            var colors = powerupButtons[i].colors;
            colors.normalColor = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);
            powerupButtons[i].colors = colors;
        }
    }

    private void HandlePowerupActivated(PowerupType type)
    {
        int index = (int)type;
        if (index < timerOverlays.Length && timerOverlays[index] != null && _maxDurations[index] > 0f)
        {
            timerOverlays[index].gameObject.SetActive(true);
            timerOverlays[index].fillAmount = 1f;
        }
        RefreshAffordability();
    }

    private void HandlePowerupExpired(PowerupType type)
    {
        int index = (int)type;
        if (index < timerOverlays.Length && timerOverlays[index] != null)
        {
            timerOverlays[index].fillAmount = 0f;
            timerOverlays[index].gameObject.SetActive(false);
        }
        RefreshAffordability();
    }

    private void HandleTimerTick(PowerupType type, float remaining)
    {
        int index = (int)type;
        if (index < timerOverlays.Length && timerOverlays[index] != null && _maxDurations[index] > 0f)
            timerOverlays[index].fillAmount = remaining / _maxDurations[index];
    }
}
