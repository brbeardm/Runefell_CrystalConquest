using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text shooterCountText;
    [SerializeField] private Text playerHPText;

    [Header("Hero Crystal Progress")]
    [SerializeField] private Slider heroCrystalSlider;

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

    private void OnEnable()
    {
        GameManager.OnScoreChanged += UpdateScore;
        GameManager.OnGameOver += ShowGameOver;
        GameManager.OnPauseToggled += UpdatePauseButton;
        WaveSpawner.OnWaveStarted += UpdateWave;
        ShooterManager.OnShooterCountChanged += UpdateShooterCount;
        ShooterHealth.OnPlayerHPChanged += UpdatePlayerHP;
        HeroCrystal.OnHeroCrystalHit += UpdateHeroCrystalProgress;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnGameOver -= ShowGameOver;
        GameManager.OnPauseToggled -= UpdatePauseButton;
        WaveSpawner.OnWaveStarted -= UpdateWave;
        ShooterManager.OnShooterCountChanged -= UpdateShooterCount;
        ShooterHealth.OnPlayerHPChanged -= UpdatePlayerHP;
        HeroCrystal.OnHeroCrystalHit -= UpdateHeroCrystalProgress;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

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

        UpdateScore(0);
        UpdateShooterCount(1);
        UpdatePlayerHP(100, 100);
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
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null && GameManager.Instance != null)
            gameOverScoreText.text = $"Score: {GameManager.Instance.Score}";
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
