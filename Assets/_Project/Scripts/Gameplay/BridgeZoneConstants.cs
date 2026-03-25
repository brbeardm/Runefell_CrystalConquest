using UnityEngine;

/// <summary>
/// Static zone boundaries for the 8-unit bridge (X: -4 to +4).
/// 10 equal parts of 0.8 units each.
/// </summary>
public static class BridgeZoneConstants
{
    // Full bridge
    public const float BridgeMinX = -4.0f;
    public const float BridgeMaxX = 4.0f;
    public const float BridgeWidth = 8.0f;
    public const float PartWidth = 0.8f;

    // Hero Crystal zone (parts 1-2)
    public const float HeroCrystalMinX = -4.0f;
    public const float HeroCrystalMaxX = -2.4f;
    public const float HeroCrystalCenterX = -3.2f;

    // Enemy Horde zone (parts 3-9)
    public const float EnemyHordeMinX = -2.4f;
    public const float EnemyHordeMaxX = 3.2f;

    // Crystal Ball zone (part 10)
    public const float CrystalBallMinX = 3.2f;
    public const float CrystalBallMaxX = 4.0f;
    public const float CrystalBallCenterX = 3.6f;
}
