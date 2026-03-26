using UnityEngine;
using System;
using System.Collections;

public class HeroCrystal : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private HeroCrystalData data;

    [Header("Crystal Pieces (assign 10 child meshes, top to bottom)")]
    [SerializeField] private GameObject[] crystalPieces = new GameObject[10];

    [Header("Health Display")]
    [Tooltip("Y offset above crystal for health text.")]
    [SerializeField] private float healthTextYOffset = 3.5f;
    [SerializeField] private Color healthTextColor = Color.white;
    [SerializeField] private int healthFontSize = 50;

    /// <summary>Fired when crystal is hit (currentHits, maxHits).</summary>
    public static event Action<int, int> OnHeroCrystalHit;

    /// <summary>Fired when crystal is broken. Params: fireRateMult, damageMult, buffDuration.</summary>
    public static event Action<float, float, float> OnCrystalBroken;

    /// <summary>Fired when crystal respawns and is ready to be shot again.</summary>
    public static event Action OnCrystalRespawned;

    public static HeroCrystal Instance { get; private set; }
    public bool IsPresent => _isActive;
    public Bounds CrystalBounds => _collider != null ? _collider.bounds : new Bounds(transform.position, Vector3.one);

    private int _currentHits;
    private int _piecesDestroyed;
    private bool _isActive = true;
    private Collider _collider;
    private string _healthDisplayText;
    private GUIStyle _guiStyle;
    private GUIStyle _bgStyle;

    public int CurrentHits => _currentHits;
    public int MaxHits => _scaledHitsToFree > 0 ? _scaledHitsToFree : (data != null ? data.hitsToFree : 250);

    private int _scaledHitsToFree;
    private float _scaledBuffDuration;

    /// <summary>
    /// Called by WaveSpawner at the start of each wave to scale crystal difficulty.
    /// </summary>
    public void SetWaveScaling(int hitsToFree, float buffDuration)
    {
        _scaledHitsToFree = hitsToFree;
        _scaledBuffDuration = buffDuration;
        UpdateHealthText();
    }

    private void Awake()
    {
        Instance = this;
        _collider = GetComponent<Collider>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        UpdateHealthText();
    }

    private void OnGUI()
    {
        if (!_isActive || Camera.main == null) return;
        if (string.IsNullOrEmpty(_healthDisplayText)) return;

        Vector3 worldPos = transform.position + Vector3.up * healthTextYOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return;

        if (_guiStyle == null)
        {
            _guiStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            _bgStyle = new GUIStyle(GUI.skin.box);
        }

        _guiStyle.fontSize = healthFontSize;
        _guiStyle.normal.textColor = healthTextColor;

        float guiY = Screen.height - screenPos.y;
        Vector2 size = _guiStyle.CalcSize(new GUIContent(_healthDisplayText));
        size.x += 10f;
        size.y += 4f;

        Rect rect = new Rect(screenPos.x - size.x * 0.5f, guiY - size.y, size.x, size.y);
        GUI.Box(rect, GUIContent.none, _bgStyle);
        GUI.Label(rect, _healthDisplayText, _guiStyle);
    }

    private void UpdateHealthText()
    {
        int max = MaxHits;
        int remaining = max - _currentHits;
        _healthDisplayText = $"{remaining} / {max}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;

        if (other.CompareTag("Projectile"))
        {
            _currentHits++;
            int max = MaxHits;
            OnHeroCrystalHit?.Invoke(_currentHits, max);

            // Destroy crystal pieces progressively
            int hitsPerPiece = data != null ? data.hitsPerPiece : 5;
            int shouldBeDestroyed = Mathf.Min(_currentHits / hitsPerPiece, crystalPieces.Length);
            while (_piecesDestroyed < shouldBeDestroyed)
            {
                if (_piecesDestroyed < crystalPieces.Length && crystalPieces[_piecesDestroyed] != null)
                    crystalPieces[_piecesDestroyed].SetActive(false);
                _piecesDestroyed++;
            }

            UpdateHealthText();

            if (_currentHits >= max)
            {
                ActivateBuff();
            }
        }
    }

    private void ActivateBuff()
    {
        _isActive = false;

        // Hide all crystal pieces
        foreach (var piece in crystalPieces)
        {
            if (piece != null) piece.SetActive(false);
        }

        // Disable collider
        if (_collider != null) _collider.enabled = false;

        // Get buff params from data
        float fireRateMult = data != null ? data.fireRateMultiplier : 2f;
        float damageMult = data != null ? data.damageMultiplier : 2f;
        float duration = _scaledBuffDuration > 0 ? _scaledBuffDuration : (data != null ? 30f : 30f);

        // Fire event — CrystalBuffManager picks this up
        OnCrystalBroken?.Invoke(fireRateMult, damageMult, duration);
    }

    /// <summary>
    /// Called by CrystalBuffManager when the buff expires to begin the respawn cycle.
    /// </summary>
    public void StartRespawnCycle()
    {
        float delay = data != null ? data.respawnDelay : 10f;
        StartCoroutine(RespawnAfterDelay(delay));
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RespawnCrystal();
    }

    private void RespawnCrystal()
    {
        _currentHits = 0;
        _piecesDestroyed = 0;
        _isActive = true;

        foreach (var piece in crystalPieces)
        {
            if (piece != null) piece.SetActive(true);
        }

        UpdateHealthText();

        if (_collider != null) _collider.enabled = true;

        OnCrystalRespawned?.Invoke();
    }
}
