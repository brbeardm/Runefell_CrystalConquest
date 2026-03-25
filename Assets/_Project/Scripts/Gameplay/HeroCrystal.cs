using UnityEngine;
using System;
using System.Collections;

public class HeroCrystal : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private HeroCrystalData data;

    [Header("Crystal Pieces (assign 10 child meshes, top to bottom)")]
    [SerializeField] private GameObject[] crystalPieces = new GameObject[10];

    [Header("Hero Inside Crystal")]
    [SerializeField] private GameObject heroMeshInside;
    [SerializeField] private Renderer[] heroRenderers;

    [Header("Health Display")]
    [Tooltip("Y offset above crystal for health text.")]
    [SerializeField] private float healthTextYOffset = 3.5f;
    [SerializeField] private float healthTextScale = 0.02f;
    [SerializeField] private Color healthTextColor = Color.white;

    public static event Action<int, int> OnHeroCrystalHit;  // current, max
    public static event Action OnHeroFreed;
    public static event Action OnHeroExpired;

    public static HeroCrystal Instance { get; private set; }
    public bool IsPresent => _isActive;
    public Bounds CrystalBounds => _collider != null ? _collider.bounds : new Bounds(transform.position, Vector3.one);

    private int _currentHits;
    private int _piecesDestroyed;
    private bool _isActive = true;
    private Collider _collider;
    private TextMesh _healthText;
    private GameObject _healthTextObj;

    public int CurrentHits => _currentHits;
    public int MaxHits => data != null ? data.hitsToFree : 250;

    private void Awake()
    {
        Instance = this;
        _collider = GetComponent<Collider>();
        CreateHealthText();
    }

    private void OnEnable()
    {
        HeroPromotion.OnTierChanged += HandleTierChanged;
    }

    private void OnDisable()
    {
        HeroPromotion.OnTierChanged -= HandleTierChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void HandleTierChanged(int newTier)
    {
        // When player demotes back to base tier, respawn the crystal after a delay
        if (newTier == 0 && !_isActive)
        {
            OnHeroExpired?.Invoke();
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(data != null ? data.respawnDelay : 5f);
        RespawnCrystal();
    }

    private void Start()
    {
        if (heroMeshInside != null)
            heroMeshInside.SetActive(true);

        SetHeroOpacity(0.05f);
        UpdateHealthText();
    }

    private void LateUpdate()
    {
        // Make health text face the camera
        if (_healthTextObj != null && _healthTextObj.activeSelf)
        {
            var cam = Camera.main;
            if (cam != null)
                _healthTextObj.transform.rotation = Quaternion.LookRotation(
                    _healthTextObj.transform.position - cam.transform.position);
        }
    }

    private void CreateHealthText()
    {
        _healthTextObj = new GameObject("CrystalHealthText");
        _healthTextObj.transform.SetParent(transform);
        _healthTextObj.transform.localPosition = new Vector3(0f, healthTextYOffset, 0f);
        _healthTextObj.transform.localScale = Vector3.one * healthTextScale;

        _healthText = _healthTextObj.AddComponent<TextMesh>();
        _healthText.alignment = TextAlignment.Center;
        _healthText.anchor = TextAnchor.MiddleCenter;
        _healthText.fontSize = 120;
        _healthText.characterSize = 0.5f;
        _healthText.color = healthTextColor;
        _healthText.fontStyle = FontStyle.Bold;
    }

    private void UpdateHealthText()
    {
        if (_healthText == null || data == null) return;

        int remaining = data.hitsToFree - _currentHits;
        _healthText.text = $"{remaining} / {data.hitsToFree}";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;
        if (data == null) return;

        if (other.CompareTag("Projectile"))
        {
            _currentHits++;
            OnHeroCrystalHit?.Invoke(_currentHits, data.hitsToFree);

            float progress = (float)_currentHits / data.hitsToFree;

            // Destroy crystal pieces from top — each piece takes hitsPerPiece shots
            int shouldBeDestroyed = Mathf.Min(_currentHits / data.hitsPerPiece, crystalPieces.Length);
            while (_piecesDestroyed < shouldBeDestroyed)
            {
                if (_piecesDestroyed < crystalPieces.Length && crystalPieces[_piecesDestroyed] != null)
                    crystalPieces[_piecesDestroyed].SetActive(false);
                _piecesDestroyed++;
            }

            // Reveal hero inside as crystal breaks
            SetHeroOpacity(Mathf.Lerp(0.05f, 1f, progress));

            UpdateHealthText();

            if (_currentHits >= data.hitsToFree)
            {
                FreeHero();
            }
        }
    }

    private void SetHeroOpacity(float alpha)
    {
        if (heroRenderers == null) return;

        foreach (var rend in heroRenderers)
        {
            if (rend == null) continue;

            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
            }
        }
    }

    private void FreeHero()
    {
        _isActive = false;

        foreach (var piece in crystalPieces)
        {
            if (piece != null) piece.SetActive(false);
        }
        
        // Deactivate the visual hero mesh inside the crystal
        if (heroMeshInside != null)
            heroMeshInside.SetActive(false);
            
        if (_healthTextObj != null)
            _healthTextObj.SetActive(false);

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        OnHeroFreed?.Invoke();

        // Promote the player to the next hero tier
        var promotion = UnityEngine.Object.FindAnyObjectByType<HeroPromotion>();
        if (promotion != null)
        {
            promotion.Promote();
        }
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

        if (heroMeshInside != null)
            heroMeshInside.SetActive(true);
        SetHeroOpacity(0.05f);

        if (_healthTextObj != null)
            _healthTextObj.SetActive(true);
        UpdateHealthText();

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }
}
