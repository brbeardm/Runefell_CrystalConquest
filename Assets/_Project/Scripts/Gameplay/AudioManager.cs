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

    [Header("Boss Animated")]
    [SerializeField] private AudioClip bossRoarSfx;
    [SerializeField] private AudioClip bossIntimidationSfx;

    [Header("Revive")]
    [SerializeField] private AudioClip reviveBlastSfx;
    [SerializeField] private float reviveDuckDuration = 1.2f;

    [Header("Combat")]
    [SerializeField] private AudioClip playerDamageSfx;
    [SerializeField] private AudioClip enemyDeathSfx;

    private AudioSource _audioSource;
    private AudioSource _reviveSource;
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

        // Dedicated source for revive blast — ignores listener volume so it cuts through the duck
        _reviveSource = gameObject.AddComponent<AudioSource>();
        _reviveSource.playOnAwake = false;
        _reviveSource.spatialBlend = 0f;
        _reviveSource.ignoreListenerVolume = true;
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
        ReviveManager.OnReviveStarted += HandleReviveBlast;
        AnimatedBossBehavior.OnBossRoar += HandleBossRoar;
        AnimatedBossBehavior.OnBossIntimidationKill += HandleBossIntimidation;
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
        ReviveManager.OnReviveStarted -= HandleReviveBlast;
        AnimatedBossBehavior.OnBossRoar -= HandleBossRoar;
        AnimatedBossBehavior.OnBossIntimidationKill -= HandleBossIntimidation;
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

    private void HandleBossRoar() => PlayClip(bossRoarSfx);
    private void HandleBossIntimidation() => PlayClip(bossIntimidationSfx);

    private void HandleReviveBlast()
    {
        if (reviveBlastSfx != null && _reviveSource != null)
            _reviveSource.PlayOneShot(reviveBlastSfx);
        StartCoroutine(DuckAudioForBlast());
    }

    private System.Collections.IEnumerator DuckAudioForBlast()
    {
        float savedVolume = AudioListener.volume;
        AudioListener.volume = 0.05f; // near-silence for everything else

        float elapsed = 0f;
        while (elapsed < reviveDuckDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade back in over 0.3s
        float fadeTime = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeTime)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            AudioListener.volume = Mathf.Lerp(0.05f, savedVolume, fadeElapsed / fadeTime);
            yield return null;
        }
        AudioListener.volume = savedVolume;
    }
}
