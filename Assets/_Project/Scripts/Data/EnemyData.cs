using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Runefell/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Orc";
    public bool isBoss;

    [Header("Stats")]
    [Tooltip("Total hit points.")]
    public int maxHealth = 30;

    [Tooltip("Walk speed toward the player (units/sec).")]
    public float moveSpeed = 2f;

    [Tooltip("Damage dealt to the player on contact.")]
    public int contactDamage = 10;

    [Tooltip("Boss contact = instant game over (kills player + all clones).")]
    public bool isInstantKill;

    [Tooltip("HP regenerated per second (0 = none). Used by Crystal Troll boss.")]
    public float healthRegenRate;

    [Header("Visuals")]
    [Tooltip("Uniform scale multiplier. Bosses are larger (5x).")]
    public float scaleMultiplier = 1f;

    [Header("Rewards")]
    [Tooltip("Score awarded when killed.")]
    public int scoreValue = 10;

    [Tooltip("Chance (0-1) to drop a power-up on death.")]
    [Range(0f, 1f)]
    public float powerUpDropChance = 0.05f;
}
