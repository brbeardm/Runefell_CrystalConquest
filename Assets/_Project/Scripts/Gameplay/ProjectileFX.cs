using UnityEngine;

/// <summary>
/// Attach to the projectile prefab. Handles:
/// 1. Muzzle flash — brief bright point light burst on fire
/// 2. In-flight glow — dimmer point light that travels with the shard
/// 3. Optional short particle trail
/// </summary>
public class ProjectileFX : MonoBehaviour
{
    [Header("Muzzle Flash")]
    [Tooltip("Intensity of the flash burst when fired.")]
    [SerializeField] private float flashIntensity = 8f;
    [Tooltip("How quickly the flash fades (seconds).")]
    [SerializeField] private float flashDuration = 0.08f;
    [Tooltip("Flash light color.")]
    [SerializeField] private Color flashColor = new Color(0.6f, 0.9f, 1f, 1f); // ice-blue

    [Header("In-Flight Glow")]
    [Tooltip("Steady glow intensity while the shard is flying.")]
    [SerializeField] private float glowIntensity = 2f;
    [Tooltip("Range of the glow light.")]
    [SerializeField] private float glowRange = 3f;

    [Header("Trail")]
    [Tooltip("Optional TrailRenderer on this projectile.")]
    [SerializeField] private TrailRenderer trail;

    private Light _light;
    private float _flashTimer;

    private void Awake()
    {
        // Create a single point light — doubles as flash then settles to glow
        _light = GetComponentInChildren<Light>();
        if (_light == null)
        {
            var lightGO = new GameObject("ShardLight");
            lightGO.transform.SetParent(transform, false);
            _light = lightGO.AddComponent<Light>();
        }

        _light.type = LightType.Point;
        _light.color = flashColor;
        _light.range = glowRange;
        _light.shadows = LightShadows.None;
    }

    private void OnEnable()
    {
        // Start with bright flash
        _flashTimer = flashDuration;
        _light.intensity = flashIntensity;

        // Reset trail so it doesn't streak from the last position
        if (trail != null)
            trail.Clear();
    }

    private void Update()
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            // Lerp from flash intensity down to glow intensity
            float t = Mathf.Clamp01(_flashTimer / flashDuration);
            _light.intensity = Mathf.Lerp(glowIntensity, flashIntensity, t);
        }
    }

    private void OnDisable()
    {
        // Reset for next pool reuse
        if (_light != null)
            _light.intensity = 0f;
        if (trail != null)
            trail.Clear();
    }
}
