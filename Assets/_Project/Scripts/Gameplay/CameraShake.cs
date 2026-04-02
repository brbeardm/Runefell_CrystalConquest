using UnityEngine;
using System.Collections;

/// <summary>
/// Reusable camera shake. Attach to the main camera.
/// Call CameraShake.Shake() from anywhere to trigger a shake.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 _originalLocalPos;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _originalLocalPos = transform.localPosition;
    }

    /// <summary>
    /// Trigger a camera shake with intensity decay.
    /// </summary>
    /// <param name="intensity">Max displacement in units.</param>
    /// <param name="duration">How long the shake lasts.</param>
    /// <param name="sustained">If true, shake stays at full intensity (no decay).</param>
    public static void Shake(float intensity = 0.3f, float duration = 0.4f, bool sustained = false)
    {
        if (Instance != null)
            Instance.DoShake(intensity, duration, sustained);
    }

    private void DoShake(float intensity, float duration, bool sustained)
    {
        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration, sustained));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration, bool sustained)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float strength = sustained ? 1f : (1f - t);

            float offsetX = Random.Range(-1f, 1f) * intensity * strength;
            float offsetY = Random.Range(-1f, 1f) * intensity * strength;

            transform.localPosition = _originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = _originalLocalPos;
        _shakeRoutine = null;
    }
}
