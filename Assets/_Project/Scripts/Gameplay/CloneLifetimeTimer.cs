using UnityEngine;
using System.Collections;

/// <summary>
/// Added to clones on spawn. Counts down lifetime, flashes the clone
/// in the last 5 seconds, then destroys it via ShooterManager.
/// </summary>
public class CloneLifetimeTimer : MonoBehaviour
{
    [SerializeField] private float lifetime = 25f;
    [SerializeField] private float flashStartTime = 5f;
    [SerializeField] private float flashSpeed = 6f;

    private Renderer[] _renderers;
    private float _timeRemaining;

    private void Start()
    {
        _timeRemaining = lifetime;
        _renderers = GetComponentsInChildren<Renderer>();
        StartCoroutine(LifetimeCountdown());
    }

    private IEnumerator LifetimeCountdown()
    {
        // Normal phase
        while (_timeRemaining > flashStartTime)
        {
            _timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Flash phase — last 5 seconds
        while (_timeRemaining > 0f)
        {
            _timeRemaining -= Time.deltaTime;

            // Toggle visibility rapidly
            float alpha = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
            alpha = Mathf.Lerp(0.2f, 1f, alpha);
            SetAlpha(alpha);

            yield return null;
        }

        // Time's up — destroy clone
        SetAlpha(1f);

        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RemoveShooter(gameObject);

        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (_renderers == null) return;

        foreach (var rend in _renderers)
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
}
