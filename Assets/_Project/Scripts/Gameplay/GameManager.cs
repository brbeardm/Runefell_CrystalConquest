using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Paused, GameOver, Victory }

    public static GameManager Instance { get; private set; }

    public static event Action OnGameStarted;
    public static event Action OnGameOver;
    public static event Action<int> OnScoreChanged;
    public static event Action<bool> OnPauseToggled; // true = paused
    public static event Action OnVictory;

    public GameState State { get; private set; } = GameState.Playing;
    public int Score { get; private set; }

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
        Enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        State = GameState.Playing;
        Score = 0;
        OnScoreChanged?.Invoke(Score);
        OnGameStarted?.Invoke();
    }

    public void TogglePause()
    {
        if (State == GameState.GameOver) return;

        if (State == GameState.Paused)
        {
            State = GameState.Playing;
            Time.timeScale = 1f;
        }
        else
        {
            State = GameState.Paused;
            Time.timeScale = 0f;
        }

        OnPauseToggled?.Invoke(State == GameState.Paused);
    }

    public bool IsPaused => State == GameState.Paused;

    public void EndGame()
    {
        if (State == GameState.GameOver || State == GameState.Victory) return;
        Time.timeScale = 1f;
        State = GameState.GameOver;
        OnGameOver?.Invoke();
    }

    public void Victory()
    {
        if (State == GameState.GameOver || State == GameState.Victory) return;
        State = GameState.Victory;
        OnVictory?.Invoke();
    }

    public void RestartGame()
    {
        // Clear static event subscribers to avoid duplicates on reload
        Time.timeScale = 1f;
        OnGameStarted = null;
        OnGameOver = null;
        OnScoreChanged = null;
        OnPauseToggled = null;
        OnVictory = null;
        Instance = null;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddScore(int amount)
    {
        if (State == GameState.GameOver) return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    private void HandleEnemyDied(Enemy enemy)
    {
        if (enemy.Data != null)
            AddScore(enemy.Data.scoreValue);
    }
}
