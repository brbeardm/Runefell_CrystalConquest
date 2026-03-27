using System;
using UnityEngine;

/// <summary>
/// Persistent dual-currency wallet backed by PlayerPrefs.
/// Gems (blue/survival) and Runes (gold/offense).
/// </summary>
public static class PlayerWallet
{
    private const string GemsKey = "Runefell_Gems";
    private const string RunesKey = "Runefell_Runes";
    private const int DefaultSoftCap = 500;

    public static int SoftCap { get; set; } = DefaultSoftCap;

    /// <summary>Fired whenever either currency changes (gems, runes).</summary>
    public static event Action<int, int> OnCurrencyChanged;

    public static int Gems
    {
        get => PlayerPrefs.GetInt(GemsKey, 0);
        private set
        {
            PlayerPrefs.SetInt(GemsKey, Mathf.Clamp(value, 0, SoftCap));
            PlayerPrefs.Save();
        }
    }

    public static int Runes
    {
        get => PlayerPrefs.GetInt(RunesKey, 0);
        private set
        {
            PlayerPrefs.SetInt(RunesKey, Mathf.Clamp(value, 0, SoftCap));
            PlayerPrefs.Save();
        }
    }

    public static void AddGems(int amount)
    {
        Gems += amount;
        OnCurrencyChanged?.Invoke(Gems, Runes);
    }

    public static void AddRunes(int amount)
    {
        Runes += amount;
        OnCurrencyChanged?.Invoke(Gems, Runes);
    }

    public static bool SpendGems(int amount)
    {
        if (Gems < amount) return false;
        Gems -= amount;
        OnCurrencyChanged?.Invoke(Gems, Runes);
        return true;
    }

    public static bool SpendRunes(int amount)
    {
        if (Runes < amount) return false;
        Runes -= amount;
        OnCurrencyChanged?.Invoke(Gems, Runes);
        return true;
    }

    public static void ResetEvents()
    {
        OnCurrencyChanged = null;
    }
}
