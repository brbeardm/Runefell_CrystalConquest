using UnityEngine;
using System.Collections;

/// <summary>
/// Wave 7 — Crystal Wraith Boss. Phases in and out of visibility.
/// When phased out: collider disabled, semi-transparent, projectiles pass through.
/// Cycles: 2s visible, 1.5s phased.
/// </summary>
public class CrystalWraithBossBehavior : BossBehavior
{
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float phasedDuration = 1.5f;
    [SerializeField] private float phasedAlpha = 0.15f;

    private Collider _collider;
    private Renderer _renderer;
    private bool _isPhased;

    protected override void Awake()
    {
        base.Awake();
        _collider = GetComponent<Collider>();
        _renderer = GetComponentInChildren<Renderer>();
    }

    public override void OnSpawn()
    {
        StartCoroutine(PhaseCycle());
        Debug.Log("[Boss] Crystal Wraith Boss phases into existence!");
    }

    private IEnumerator PhaseCycle()
    {
        while (enemy != null && !enemy.IsDead)
        {
            // Visible phase
            SetPhased(false);
            yield return new WaitForSeconds(visibleDuration);

            if (enemy == null || enemy.IsDead) yield break;

            // Phased (invulnerable)
            SetPhased(true);
            yield return new WaitForSeconds(phasedDuration);
        }
    }

    private void SetPhased(bool phased)
    {
        _isPhased = phased;

        if (_collider != null)
            _collider.enabled = !phased;

        if (_renderer != null)
        {
            foreach (var mat in _renderer.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = phased ? phasedAlpha : 1f;
                    mat.SetColor("_BaseColor", c);
                }
            }
        }
    }

    public bool IsPhased => _isPhased;
}
