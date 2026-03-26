using UnityEngine;

/// <summary>
/// Wave 3 — Wraith Boss. Fast movement with erratic lateral weaving to be harder to hit.
/// Speed is set via EnemyData/WaveScalingConfig (bossSpeed = 2.5).
/// This script adds lateral weaving on top of the base movement.
/// </summary>
public class WraithBossBehavior : BossBehavior
{
    [SerializeField] private float weaveAmplitude = 1.5f;
    [SerializeField] private float weaveFrequency = 2f;

    private float _weaveTimer;

    public override void OnSpawn()
    {
        _weaveTimer = Random.Range(0f, Mathf.PI * 2f);
        Debug.Log("[Boss] Wraith Boss glides onto the bridge!");
    }

    private void Update()
    {
        if (enemy == null || enemy.IsDead) return;

        _weaveTimer += Time.deltaTime * weaveFrequency;
        float weaveOffset = Mathf.Sin(_weaveTimer) * weaveAmplitude * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.x += weaveOffset;
        pos.x = Mathf.Clamp(pos.x, BridgeZoneConstants.EnemyHordeMinX, BridgeZoneConstants.EnemyHordeMaxX);
        transform.position = pos;
    }
}
