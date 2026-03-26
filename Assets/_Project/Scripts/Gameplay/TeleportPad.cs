using UnityEngine;

/// <summary>
/// Visual teleport pad under the HeroCrystal.
/// Color cycles Red → Orange → Yellow → Green during crystal respawn.
/// Green = crystal available. Dark = buff active (crystal not present).
/// </summary>
public class TeleportPad : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color waitingStartColor = Color.red;
    [SerializeField] private Color waitingMidColor = new Color(1f, 0.5f, 0f); // orange
    [SerializeField] private Color waitingLateColor = Color.yellow;
    [SerializeField] private Color inactiveColor = new Color(0.1f, 0.1f, 0.1f);

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;

    private Renderer _renderer;
    private Material _material;

    private enum PadState { Ready, BuffActive, Respawning }
    private PadState _state = PadState.Ready;
    private float _respawnDuration;
    private float _respawnElapsed;

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
            _material = _renderer.material;
    }

    private void OnEnable()
    {
        HeroCrystal.OnCrystalBroken += HandleCrystalBroken;
        HeroCrystal.OnCrystalRespawned += HandleCrystalRespawned;
        CrystalBuffManager.OnBuffExpired += HandleBuffExpired;
    }

    private void OnDisable()
    {
        HeroCrystal.OnCrystalBroken -= HandleCrystalBroken;
        HeroCrystal.OnCrystalRespawned -= HandleCrystalRespawned;
        CrystalBuffManager.OnBuffExpired -= HandleBuffExpired;
    }

    private void Start()
    {
        SetColor(readyColor);
        _state = PadState.Ready;
    }

    private void HandleCrystalBroken(float fr, float dm, float dur)
    {
        _state = PadState.BuffActive;
        SetColor(inactiveColor);
    }

    private void HandleBuffExpired()
    {
        // Buff ended — start respawn color cycle
        _state = PadState.Respawning;
        _respawnElapsed = 0f;

        // Get respawn delay from crystal data
        if (HeroCrystal.Instance != null)
        {
            // The crystal's StartRespawnCycle uses data.respawnDelay
            // We read it indirectly — default to 10s
            _respawnDuration = 10f;
        }
        else
        {
            _respawnDuration = 10f;
        }
    }

    private void HandleCrystalRespawned()
    {
        _state = PadState.Ready;
        SetColor(readyColor);
    }

    private void Update()
    {
        if (_material == null) return;

        if (_state == PadState.Ready)
        {
            // Pulse green
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            SetColor(readyColor * pulse);
        }
        else if (_state == PadState.Respawning)
        {
            _respawnElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_respawnElapsed / _respawnDuration);

            // Red (0-0.33) → Orange (0.33-0.66) → Yellow (0.66-0.9) → Green (0.9-1.0)
            Color c;
            if (t < 0.33f)
                c = Color.Lerp(waitingStartColor, waitingMidColor, t / 0.33f);
            else if (t < 0.66f)
                c = Color.Lerp(waitingMidColor, waitingLateColor, (t - 0.33f) / 0.33f);
            else
                c = Color.Lerp(waitingLateColor, readyColor, (t - 0.66f) / 0.34f);

            // Pulse faster as it gets closer to green
            float speed = Mathf.Lerp(1f, 4f, t);
            float pulse = 1f + Mathf.Sin(Time.time * speed) * pulseIntensity;
            SetColor(c * pulse);
        }
    }

    private void SetColor(Color c)
    {
        if (_material == null) return;

        if (_material.HasProperty("_BaseColor"))
            _material.SetColor("_BaseColor", c);
        if (_material.HasProperty("_EmissionColor"))
        {
            _material.EnableKeyword("_EMISSION");
            _material.SetColor("_EmissionColor", c * 1.5f);
        }
    }
}
