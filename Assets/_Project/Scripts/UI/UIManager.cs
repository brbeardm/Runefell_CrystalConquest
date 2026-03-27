using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text waveCounterText;
    [SerializeField] private Text shooterCountText;
    [SerializeField] private Text playerHPText;

    [Header("Hero Crystal Progress")]
    [SerializeField] private Slider heroCrystalSlider;

    [Header("Buff Timer")]
    [SerializeField] private Text buffTimerText;

    [Header("Breather Countdown")]
    [SerializeField] private Text breatherCountdownText;

    [Header("Pause")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Text pauseButtonText;

    [Header("Mute")]
    [SerializeField] private Button muteButton;
    [SerializeField] private Text muteButtonText;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Button restartButton;

    [Header("Victory Panel")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text victoryTitleText;
    [SerializeField] private Text victoryScoreText;
    [SerializeField] private Button victoryRestartButton;

    private int _totalWaves;

    private void OnEnable()
    {
        GameManager.OnScoreChanged += UpdateScore;
        GameManager.OnGameOver += ShowGameOver;
        GameManager.OnVictory += ShowVictory;
        GameManager.OnPauseToggled += UpdatePauseButton;
        WaveSpawner.OnWaveStarted += UpdateWave;
        WaveSpawner.OnBreatherStarted += StartBreatherCountdown;
        ShooterManager.OnShooterCountChanged += UpdateShooterCount;
        ShooterHealth.OnPlayerHPChanged += UpdatePlayerHP;
        HeroCrystal.OnHeroCrystalHit += UpdateHeroCrystalProgress;
        CrystalBuffManager.OnBuffActivated += ShowBuffTimer;
        CrystalBuffManager.OnBuffTimerTick += UpdateBuffTimer;
        CrystalBuffManager.OnBuffExpired += HideBuffTimer;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnGameOver -= ShowGameOver;
        GameManager.OnVictory -= ShowVictory;
        GameManager.OnPauseToggled -= UpdatePauseButton;
        WaveSpawner.OnWaveStarted -= UpdateWave;
        WaveSpawner.OnBreatherStarted -= StartBreatherCountdown;
        ShooterManager.OnShooterCountChanged -= UpdateShooterCount;
        ShooterHealth.OnPlayerHPChanged -= UpdatePlayerHP;
        HeroCrystal.OnHeroCrystalHit -= UpdateHeroCrystalProgress;
        CrystalBuffManager.OnBuffActivated -= ShowBuffTimer;
        CrystalBuffManager.OnBuffTimerTick -= UpdateBuffTimer;
        CrystalBuffManager.OnBuffExpired -= HideBuffTimer;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (victoryRestartButton != null)
            victoryRestartButton.onClick.AddListener(OnRestartClicked);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseClicked);

        if (muteButton != null)
            muteButton.onClick.AddListener(OnMuteClicked);

        UpdatePauseButton(false);
        UpdateMuteButton();

        if (heroCrystalSlider != null)
        {
            heroCrystalSlider.minValue = 0;
            heroCrystalSlider.maxValue = 1;
            heroCrystalSlider.value = 0;
        }

        if (buffTimerText != null)
            buffTimerText.gameObject.SetActive(false);

        if (breatherCountdownText != null)
            breatherCountdownText.gameObject.SetActive(false);

        // Cache total waves for wave counter display
        var spawner = FindAnyObjectByType<WaveSpawner>();
        _totalWaves = spawner != null ? spawner.TotalWaves : 9;

        UpdateScore(0);
        UpdateShooterCount(1);
        UpdatePlayerHP(100, 100);

        if (waveCounterText != null)
            waveCounterText.text = "";
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private void UpdateWave(int waveIndex, string waveName)
    {
        if (waveText != null)
            waveText.text = waveName;

        if (waveCounterText != null)
            waveCounterText.text = $"Wave {waveIndex + 1} / {_totalWaves}";

        // Hide breather countdown when wave starts
        if (breatherCountdownText != null)
            breatherCountdownText.gameObject.SetActive(false);
    }

    private void UpdateShooterCount(int count)
    {
        if (shooterCountText != null)
            shooterCountText.text = $"Shooters: {count}";
    }

    private void UpdatePlayerHP(int current, int max)
    {
        if (playerHPText != null)
            playerHPText.text = $"HP: {current}";
    }

    private void UpdateHeroCrystalProgress(int current, int max)
    {
        if (heroCrystalSlider != null && max > 0)
            heroCrystalSlider.value = (float)current / max;
    }

    private void ShowGameOver()
    {
        // RevivePanel handles the death screen when ReviveManager is present
        if (ReviveManager.Instance != null) return;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null && GameManager.Instance != null)
            gameOverScoreText.text = $"Score: {GameManager.Instance.Score}";
    }

    // ── Victory Screen ──────────────────────────────────

    private void ShowVictory()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (victoryTitleText != null)
            victoryTitleText.text = "YOU ARE THE CRYSTAL KING!";

        if (victoryScoreText != null && GameManager.Instance != null)
            victoryScoreText.text = $"Final Score: {GameManager.Instance.Score}";
    }

    // ── Buff Timer ──────────────────────────────────────

    private void ShowBuffTimer(float duration)
    {
        if (buffTimerText != null)
        {
            buffTimerText.gameObject.SetActive(true);
            buffTimerText.text = $"BUFF {duration:F1}s";
        }
    }

    private void UpdateBuffTimer(float remaining)
    {
        if (buffTimerText != null)
            buffTimerText.text = $"BUFF {Mathf.Max(0f, remaining):F1}s";
    }

    private void HideBuffTimer()
    {
        if (buffTimerText != null)
            buffTimerText.gameObject.SetActive(false);
    }

    // ── Breather Countdown ──────────────────────────────

    private void StartBreatherCountdown(float duration)
    {
        StopCoroutine(nameof(RunBreatherCountdown));
        StartCoroutine(RunBreatherCountdown(duration));
    }

    private IEnumerator RunBreatherCountdown(float duration)
    {
        if (breatherCountdownText == null) yield break;

        breatherCountdownText.gameObject.SetActive(true);
        float remaining = duration;

        while (remaining > 0f)
        {
            breatherCountdownText.text = $"Next wave in {Mathf.CeilToInt(remaining)}...";
            remaining -= Time.deltaTime;
            yield return null;
        }

        breatherCountdownText.gameObject.SetActive(false);
    }

    private void OnPauseClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TogglePause();
    }

    private void UpdatePauseButton(bool isPaused)
    {
        if (pauseButtonText != null)
            pauseButtonText.text = isPaused ? "\u25B6" : "\u275A\u275A";

        AudioListener.pause = isPaused;
    }

    private void OnRestartClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private bool _isMuted;

    private void OnMuteClicked()
    {
        _isMuted = !_isMuted;
        AudioListener.volume = _isMuted ? 0f : 1f;
        UpdateMuteButton();
    }

    private void UpdateMuteButton()
    {
        if (muteButtonText != null)
            muteButtonText.text = _isMuted ? "\u266A" : "\u266B";
    }
}
