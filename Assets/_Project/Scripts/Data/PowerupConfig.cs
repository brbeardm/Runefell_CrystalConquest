using UnityEngine;
using System;

public enum PowerupType
{
    // Gem (survival)
    HealPulse,
    ShieldRune,
    IronSkin,
    SecondWind,
    // Rune (offense)
    RapidFire,
    CrystalFury,
    ArmorPiercing,
    CloneSurge
}

public enum CurrencyType { Gem, Rune }

[CreateAssetMenu(fileName = "PowerupConfig", menuName = "Runefell/Powerup Config")]
public class PowerupConfig : ScriptableObject
{
    public PowerupDef[] powerups = new PowerupDef[8];

    [Serializable]
    public class PowerupDef
    {
        public PowerupType type;
        public string displayName;
        public CurrencyType currencyType;
        public int cost;
        public float duration;      // 0 = instant or permanent-until-triggered
        public float effectValue;   // contextual: HP amount, multiplier, hit count, clone count
        public Sprite icon;
        [TextArea] public string tooltip;
    }

    public PowerupDef GetDef(PowerupType type)
    {
        return Array.Find(powerups, p => p.type == type);
    }
}
