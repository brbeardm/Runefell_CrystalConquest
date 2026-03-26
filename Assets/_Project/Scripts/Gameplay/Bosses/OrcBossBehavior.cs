using UnityEngine;

/// <summary>
/// Wave 1 — Orc Boss. Big slow tank. No special ability. Intro to bosses.
/// </summary>
public class OrcBossBehavior : BossBehavior
{
    public override void OnSpawn()
    {
        Debug.Log("[Boss] Orc Boss has entered the bridge!");
    }
}
