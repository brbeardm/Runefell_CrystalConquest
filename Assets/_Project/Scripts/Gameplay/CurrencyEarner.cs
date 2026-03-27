using UnityEngine;

/// <summary>
/// Awards gems and runes based on gameplay events.
/// Wave complete: +2 each. Boss kill: +1 each. Crystal orb: +1 alternating.
/// </summary>
public class CurrencyEarner : MonoBehaviour
{
    public static CurrencyEarner Instance { get; private set; }

    private bool _orbAlternator; // false = gem, true = rune

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        WaveSpawner.OnWaveCompleted += HandleWaveCompleted;
        Enemy.OnEnemyDied += HandleEnemyDied;
        CrystalBall.OnCrystalBallCollected += HandleCrystalBallCollected;
    }

    private void OnDisable()
    {
        WaveSpawner.OnWaveCompleted -= HandleWaveCompleted;
        Enemy.OnEnemyDied -= HandleEnemyDied;
        CrystalBall.OnCrystalBallCollected -= HandleCrystalBallCollected;
    }

    private void HandleWaveCompleted(int waveIndex)
    {
        PlayerWallet.AddGems(2);
        PlayerWallet.AddRunes(2);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        if (enemy.Data != null && enemy.Data.isBoss)
        {
            PlayerWallet.AddGems(1);
            PlayerWallet.AddRunes(1);
        }
    }

    private void HandleCrystalBallCollected()
    {
        if (_orbAlternator)
            PlayerWallet.AddRunes(1);
        else
            PlayerWallet.AddGems(1);
        _orbAlternator = !_orbAlternator;
    }
}
