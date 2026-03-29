using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages the crystal power-up buff lifecycle.
/// Listens for crystal broken → applies fire rate + damage buff → runs timer → notifies expiry.
/// </summary>
public class CrystalBuffManager : MonoBehaviour
{
    public static CrystalBuffManager Instance { get; private set; }

    /// <summary>Fired when buff activates (duration in seconds).</summary>
    public static event Action<float> OnBuffActivated;

    /// <summary>Fired when buff expires.</summary>
    public static event Action OnBuffExpired;

    /// <summary>Fired each frame while buff is active (remaining seconds).</summary>
    public static event Action<float> OnBuffTimerTick;

    public bool IsBuffActive { get; private set; }
    public float BuffTimeRemaining { get; private set; }

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
        HeroCrystal.OnCrystalBroken += HandleCrystalBroken;
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        HeroCrystal.OnCrystalBroken -= HandleCrystalBroken;
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();

        // Clear buff stats from player if buff was active
        if (IsBuffActive)
        {
            var player = FindAnyObjectByType<PlayerShooter>();
            if (player != null)
                player.ClearBuffStats();

            OnBuffExpired?.Invoke();
        }

        IsBuffActive = false;
        BuffTimeRemaining = 0f;
    }

    private void HandleCrystalBroken(float fireRateMult, float damageMult, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(RunBuff(fireRateMult, damageMult, duration));
    }

    private IEnumerator RunBuff(float fireRateMult, float damageMult, float duration)
    {
        IsBuffActive = true;
        BuffTimeRemaining = duration;

        // Apply buff to player
        var player = FindAnyObjectByType<PlayerShooter>();
        if (player != null)
            player.SetBuffStats(fireRateMult, damageMult);

        OnBuffActivated?.Invoke(duration);
        Debug.Log($"[CrystalBuff] Activated! {fireRateMult}x fire rate, {damageMult}x damage for {duration}s");

        // Countdown
        while (BuffTimeRemaining > 0f)
        {
            BuffTimeRemaining -= Time.deltaTime;
            OnBuffTimerTick?.Invoke(BuffTimeRemaining);
            yield return null;
        }

        BuffTimeRemaining = 0f;
        IsBuffActive = false;

        // Remove buff from player
        if (player != null)
            player.ClearBuffStats();

        OnBuffExpired?.Invoke();
        Debug.Log("[CrystalBuff] Expired!");

        // Notify crystal to start respawn
        if (HeroCrystal.Instance != null)
            HeroCrystal.Instance.StartRespawnCycle();
    }
}
