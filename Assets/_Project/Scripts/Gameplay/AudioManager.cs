using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Wave")]
    [SerializeField] private AudioClip waveStartSfx;
    [SerializeField] private AudioClip waveCompleteSfx;

    [Header("Boss")]
    [SerializeField] private AudioClip bossSpawnSfx;

    [Header("Crystal")]
    [SerializeField] private AudioClip crystalBreakSfx;
    [SerializeField] private AudioClip buffActivateSfx;
    [SerializeField] private AudioClip buffExpireSfx;

    [Header("Game State")]
    [SerializeField] private AudioClip victorySfx;
    [SerializeField] private AudioClip gameOverSfx;

    [Header("Combat")]
    [SerializeField] private AudioClip playerDamageSfx;
    [SerializeField] private AudioClip enemyDeathSfx;

    private AudioSource _audioSource;
    private int _lastKnownHP = 100;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        WaveSpawner.OnWaveStarted += HandleWaveStarted;
        WaveSpawner.OnWaveCompleted += HandleWaveCompleted;
        WaveSpawner.OnBossSpawned += HandleBossSpawned;
        HeroCrystal.OnCrystalBroken += HandleCrystalBroken;
        CrystalBuffManager.OnBuffActivated += HandleBuffActivated;
        CrystalBuffManager.OnBuffExpired += HandleBuffExpired;
        GameManager.OnVictory += HandleVictory;
        GameManager.OnGameOver += HandleGameOver;
        ShooterHealth.OnPlayerHPChanged += HandlePlayerHPChanged;
        Enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        WaveSpawner.OnWaveStarted -= HandleWaveStarted;
        WaveSpawner.OnWaveCompleted -= HandleWaveCompleted;
        WaveSpawner.OnBossSpawned -= HandleBossSpawned;
        HeroCrystal.OnCrystalBroken -= HandleCrystalBroken;
        CrystalBuffManager.OnBuffActivated -= HandleBuffActivated;
        CrystalBuffManager.OnBuffExpired -= HandleBuffExpired;
        GameManager.OnVictory -= HandleVictory;
        GameManager.OnGameOver -= HandleGameOver;
        ShooterHealth.OnPlayerHPChanged -= HandlePlayerHPChanged;
        Enemy.OnEnemyDied -= HandleEnemyDied;
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null && _audioSource != null)
            _audioSource.PlayOneShot(clip);
    }

    private void HandleWaveStarted(int waveIndex, string waveName) => PlayClip(waveStartSfx);
    private void HandleWaveCompleted(int waveIndex) => PlayClip(waveCompleteSfx);
    private void HandleBossSpawned() => PlayClip(bossSpawnSfx);
    private void HandleCrystalBroken(float fireRate, float damage, float duration) => PlayClip(crystalBreakSfx);
    private void HandleBuffActivated(float duration) => PlayClip(buffActivateSfx);
    private void HandleBuffExpired() => PlayClip(buffExpireSfx);
    private void HandleVictory() => PlayClip(victorySfx);
    private void HandleGameOver() => PlayClip(gameOverSfx);

    private void HandlePlayerHPChanged(int current, int max)
    {
        if (current < _lastKnownHP)
            PlayClip(playerDamageSfx);
        _lastKnownHP = current;
    }

    private void HandleEnemyDied(Enemy enemy) => PlayClip(enemyDeathSfx);
}
