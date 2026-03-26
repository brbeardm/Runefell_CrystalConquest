using UnityEngine;

/// <summary>
/// Abstract base for all boss behaviors. Attach alongside Enemy component on boss prefabs.
/// Hooks into Enemy damage/death events and provides override points for special abilities.
/// </summary>
public abstract class BossBehavior : MonoBehaviour
{
    protected Enemy enemy;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    protected virtual void OnEnable()
    {
        if (enemy != null)
            enemy.OnDamageTaken += HandleDamageTaken;
    }

    protected virtual void OnDisable()
    {
        if (enemy != null)
            enemy.OnDamageTaken -= HandleDamageTaken;
    }

    protected virtual void Start()
    {
        OnSpawn();
    }

    private void HandleDamageTaken(int currentHP, int maxHP)
    {
        OnTakeDamage(currentHP, maxHP);
    }

    /// <summary>Called when the boss first spawns.</summary>
    public virtual void OnSpawn() { }

    /// <summary>Called each time the boss takes damage (not on death).</summary>
    public virtual void OnTakeDamage(int currentHP, int maxHP) { }

    /// <summary>Called when the boss dies.</summary>
    public virtual void OnDie() { }
}
