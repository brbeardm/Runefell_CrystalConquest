using UnityEngine;
using System;

public class CrystalBall : MonoBehaviour
{
    [SerializeField] private CrystalBallData data;

    [Header("Visuals")]
    [Tooltip("Scale multiplier applied on spawn.")]
    [SerializeField] private float scaleMultiplier = 2f;

    [Tooltip("Crystal tint color.")]
    [SerializeField] private Color crystalColor = new Color(0.6f, 0.85f, 1f, 0.85f);

    [Tooltip("How reflective/metallic the crystal surface is (0-1).")]
    [SerializeField] private float metallic = 0.7f;

    [Tooltip("How smooth/glossy the crystal surface is (0-1).")]
    [SerializeField] private float smoothness = 0.95f;

    [Tooltip("Emission color for inner glow.")]
    [SerializeField] private Color emissionColor = new Color(0.3f, 0.6f, 1f, 1f);

    [Tooltip("Emission intensity multiplier.")]
    [SerializeField] private float emissionIntensity = 1.5f;

    /// <summary>Fired when a crystal orb is collected by the player.</summary>
    public static event Action OnCrystalBallCollected;

    private int _currentHits;
    private bool _cracked;
    private Action<GameObject> _releaseCallback;
    private Vector3 _baseScale;
    private bool _visualsApplied;

    public void Initialize(CrystalBallData ballData, Action<GameObject> releaseCallback = null)
    {
        data = ballData;
        _releaseCallback = releaseCallback;
        _currentHits = 0;
        _cracked = false;
    }

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        _currentHits = 0;
        _cracked = false;

        // Apply size
        transform.localScale = _baseScale * scaleMultiplier;

        // Apply shiny crystal material
        if (!_visualsApplied)
        {
            ApplyCrystalVisuals();
            _visualsApplied = true;
        }
    }

    private void Update()
    {
        if (_cracked || data == null) return;

        // Move toward the player (negative Z)
        transform.Translate(Vector3.back * data.moveSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cracked) return;

        // Player walks into the orb to collect it
        if (other.CompareTag("Player"))
        {
            Crack();
        }
    }

    private void ApplyCrystalVisuals()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        var mat = renderer.material;

        // Transparency mode (URP or Standard)
        if (mat.HasProperty("_Surface"))
        {
            // URP Lit: 0 = Opaque, 1 = Transparent
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f); // Alpha blend
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }
        else if (mat.HasProperty("_Mode"))
        {
            // Standard shader: 3 = Transparent
            mat.SetFloat("_Mode", 3f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }

        // Base color / tint
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", crystalColor);
        else if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", crystalColor);

        // Metallic + smoothness for that shiny crystal look
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        else if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);

        // Inner glow emission
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }
    }

    private void Crack()
    {
        _cracked = true;
        OnCrystalBallCollected?.Invoke();

        if (ShooterManager.Instance != null)
        {
            if (ShooterManager.Instance.ShooterCount < ShooterManager.Instance.MaxShooters)
            {
                // Spawn a clone
                ShooterManager.Instance.AddCloneShooter();
            }
            else
            {
                // At max clones — heal player 1 HP instead
                foreach (var health in FindObjectsByType<ShooterHealth>(FindObjectsSortMode.None))
                {
                    if (health.IsMainPlayer)
                    {
                        health.Heal(1);
                        break;
                    }
                }
            }
        }

        // Return to pool or destroy
        if (_releaseCallback != null)
            _releaseCallback(gameObject);
        else
            Destroy(gameObject);
    }

    public void SetReleaseCallback(Action<GameObject> callback)
    {
        _releaseCallback = callback;
    }
}
