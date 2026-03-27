using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Wave 9 — Crystal Necromancer Boss. The final boss.
/// At 50% HP: all enemies freeze, 20-30 dead orcs rise from the bridge surface,
/// boss heals to 75% HP, then everything resumes. Devastating.
/// </summary>
public class CrystalNecromancerBossBehavior : BossBehavior
{
    [SerializeField] private int resurrectCount = 25;
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private GameObject orcPrefab;

    private bool _hasResurrected;

    /// <summary>Fired when resurrection begins. All enemies should freeze.</summary>
    public static event Action OnNecromancerResurrection;

    /// <summary>Fired when resurrection ends. Enemies resume.</summary>
    public static event Action OnNecromancerResurrectionComplete;

    /// <summary>True while the resurrection sequence is playing.</summary>
    public static bool IsResurrecting { get; private set; }

    public static void ResetStaticState()
    {
        IsResurrecting = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.OnGameOver += HandleGameOver;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();
        IsResurrecting = false;
    }

    public override void OnSpawn()
    {
        _hasResurrected = false;
        IsResurrecting = false;
        Debug.Log("[Boss] Crystal Necromancer rises — the final test!");
    }

    public override void OnTakeDamage(int currentHP, int maxHP)
    {
        if (_hasResurrected) return;

        // Trigger at 50% HP
        if (currentHP <= maxHP / 2)
        {
            _hasResurrected = true;
            StartCoroutine(ResurrectionSequence());
        }
    }

    private IEnumerator ResurrectionSequence()
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.GameOver)
            yield break;

        Debug.Log("[Boss] NECROMANCER RESURRECTION — All enemies freeze!");

        IsResurrecting = true;
        OnNecromancerResurrection?.Invoke();

        // Heal to 75% HP
        if (enemy != null)
        {
            int healTarget = Mathf.RoundToInt(enemy.MaxHealth * 0.75f);
            enemy.SetHealth(healTarget);
        }

        // Spawn resurrected orcs rising from the bridge over the freeze duration
        if (orcPrefab != null)
        {
            float interval = freezeDuration / resurrectCount;
            for (int i = 0; i < resurrectCount; i++)
            {
                if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.GameOver)
                {
                    IsResurrecting = false;
                    yield break;
                }

                float x = UnityEngine.Random.Range(
                    BridgeZoneConstants.EnemyHordeMinX,
                    BridgeZoneConstants.EnemyHordeMaxX);
                // Spawn along the bridge length
                float z = UnityEngine.Random.Range(-2f, 25f);
                Vector3 pos = new Vector3(x, 0.25f, z);

                GameObject orc = Instantiate(orcPrefab, pos, Quaternion.identity);

                // Start orcs below the bridge and lerp up for "rising" effect
                Vector3 startPos = pos + Vector3.down * 1f;
                orc.transform.position = startPos;
                StartCoroutine(RiseFromGround(orc.transform, pos, 0.5f));

                yield return new WaitForSecondsRealtime(interval);
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(freezeDuration);
        }

        // Resume
        IsResurrecting = false;
        OnNecromancerResurrectionComplete?.Invoke();
        Debug.Log("[Boss] Resurrection complete — the horde rises!");
    }

    private IEnumerator RiseFromGround(Transform t, Vector3 targetPos, float duration)
    {
        if (t == null) yield break;

        Vector3 startPos = t.position;
        float elapsed = 0f;
        while (elapsed < duration && t != null)
        {
            elapsed += Time.unscaledDeltaTime;
            t.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        if (t != null)
            t.position = targetPos;
    }

    public override void OnDie()
    {
        // Clean up static state
        IsResurrecting = false;
    }
}
