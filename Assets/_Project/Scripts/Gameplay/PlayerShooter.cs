using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// PlayerShooter
// Crystal weapon controller for Runefall: Crystal Conquest
// - Fires crystal projectiles forward
// - Supports mouse (desktop) and touch (mobile) input
// - Uses a configurable fire rate
// - Spawns projectiles from a firePoint transform
public class PlayerShooter : MonoBehaviour
{
    /// <summary>Fired every time the player shoots. Clones listen to this to sync fire.</summary>
    public static event System.Action OnPlayerFired;

    [Header("References")]
    [Tooltip("Projectile prefab to spawn (must be a GameObject).")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("Transform where projectiles originate.")]
    [SerializeField] private Transform firePoint;

    [Header("Firing")]
    [Tooltip("Shots per second (e.g. 6 = ~0.166s between shots).")]
    [SerializeField] private float fireRate = 6f;

    [Tooltip("If true, holding input will repeatedly fire. If false, only single taps fire.")]
    [SerializeField] private bool holdToFire = true;

    [Tooltip("Initial speed applied to projectile if it contains a Rigidbody or Rigidbody2D.")]
    [SerializeField] private float projectileSpeed = 20f;

    [Header("Pooling")]
    [Tooltip("Use a basic object pool for performance. If false, Instantiate will be used.")]
    [SerializeField] private bool useSimplePool = true;

    [Tooltip("Initial pool size when using simple pool.")]
    [SerializeField] private int poolSize = 20;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSfx;

    private float _nextFireTime;
    private SimpleObjectPool _pool;
    private float _baseFireRate;
    private int _baseDamage = 10;

    // Buff system
    private bool _isBuffed;
    private float _buffFireRateMult = 1f;
    private float _buffDamageMult = 1f;

    private void Awake()
    {
        if (projectilePrefab == null)
            Debug.LogError($"{nameof(PlayerShooter)} on '{gameObject.name}' has no projectilePrefab assigned.");

        if (firePoint == null)
            Debug.LogError($"{nameof(PlayerShooter)} on '{gameObject.name}' has no firePoint assigned.");

        if (useSimplePool && projectilePrefab != null)
            _pool = new SimpleObjectPool(projectilePrefab, poolSize, transform);

        if (audioSource == null && fireSfx != null)
        {
            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Register with ShooterManager
        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RegisterShooter(gameObject);
    }

    private void Start()
    {
        // Retry registration in Start in case ShooterManager Awake runs after ours
        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RegisterShooter(gameObject);

        _baseFireRate = fireRate;
    }

    private void OnDestroy()
    {
        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RemoveShooter(gameObject);
    }
    private void Update()
    {
        if (Time.time < _nextFireTime) return;
        if (projectilePrefab == null || firePoint == null) return;
        if (IsPointerOverUI()) return;

        if (IsFiringInput())
        {
            Fire();
            float effectiveFireRate = fireRate;
            if (PowerupManager.Instance != null)
                effectiveFireRate *= PowerupManager.Instance.FireRateMultiplier;
            _nextFireTime = Time.time + (1f / Mathf.Max(0.0001f, effectiveFireRate));
        }
    }


    private bool IsFiringInput()
    {
        // Legacy input (works when Active Input Handling is "Both" or "Input Manager (Old)")
        try
        {
            // Mouse (legacy)
            if (holdToFire)
            {
                if (Input.GetMouseButton(0)) return true;
            }
            else
            {
                if (Input.GetMouseButtonDown(0)) return true;
            }

            // Touch (legacy) - fully qualified TouchPhase to avoid ambiguity with InputSystem.TouchPhase
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var t = Input.GetTouch(i);
                    if (holdToFire)
                    {
                        if (t.phase == UnityEngine.TouchPhase.Began ||
                            t.phase == UnityEngine.TouchPhase.Moved ||
                            t.phase == UnityEngine.TouchPhase.Stationary)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (t.phase == UnityEngine.TouchPhase.Began)
                            return true;
                    }
                }
            }
        }
        catch (System.InvalidOperationException)
        {
            // Legacy input disabled; fall through to new input system checks below (if enabled)
        }

#if ENABLE_INPUT_SYSTEM
        // New Input System: mouse
        if (Mouse.current != null)
        {
            if (holdToFire)
            {
                if (Mouse.current.leftButton.isPressed) return true;
            }
            else
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) return true;
            }
        }

        // New Input System: touch (optional)
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                if (!t.press.isPressed) continue;

                var phase = t.phase.ReadValue();

                if (holdToFire)
                {
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began ||
                        phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                        phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                        return true;
                }
                else
                {
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                        return true;
                }
            }
        }
#endif

        return false;
    }

    private void Fire()
    {
        GameObject go;
        if (useSimplePool && _pool != null)
        {
            // Get from pool but DON'T activate yet — position and velocity
            // must be set before SetActive(true) so the physics engine never
            // sees the projectile at the wrong position or with zero velocity.
            go = _pool.Get();
            go.SetActive(false);
        }
        else
        {
            go = Instantiate(projectilePrefab, transform);
            go.SetActive(false);
        }

        // Position and orient before activating
        go.transform.SetParent(null, true);
        go.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);

        // Apply velocity before activating so physics starts correctly on frame 1
        var rb3 = go.GetComponent<Rigidbody>();
        if (rb3 != null)
        {
            rb3.linearVelocity = Vector3.zero;
            rb3.angularVelocity = Vector3.zero;
            rb3.linearVelocity = firePoint.forward * projectileSpeed;
        }
        else
        {
            var rb2 = go.GetComponent<Rigidbody2D>();
            if (rb2 != null)
            {
                Vector2 vel2D = new Vector2(firePoint.forward.x, firePoint.forward.y).normalized * projectileSpeed;
                rb2.linearVelocity = vel2D;
            }
        }

        // Now activate — physics and triggers start from the correct state
        go.SetActive(true);

        // Pool return callback and damage
        var pooled = go.GetComponent<PooledProjectile>();
        if (pooled != null)
        {
            if (useSimplePool && _pool != null)
                pooled.SetReleaseCallback(_pool.ReturnToPool);

            float totalDamageMult = _buffDamageMult;
            if (PowerupManager.Instance != null)
                totalDamageMult *= PowerupManager.Instance.DamageMultiplier;
            pooled.Damage = Mathf.RoundToInt(_baseDamage * totalDamageMult);
        }

        if (audioSource != null && fireSfx != null)
            audioSource.PlayOneShot(fireSfx);

        OnPlayerFired?.Invoke();
    }

    public bool IsBuffed => _isBuffed;

    /// <summary>
    /// Apply crystal power-up buff (fire rate + damage multipliers).
    /// </summary>
    public void SetBuffStats(float fireRateMult, float damageMult)
    {
        _isBuffed = true;
        _buffFireRateMult = fireRateMult;
        _buffDamageMult = damageMult;
        fireRate = _baseFireRate * _buffFireRateMult;
    }

    /// <summary>
    /// Remove crystal power-up buff, restore base stats.
    /// </summary>
    public void ClearBuffStats()
    {
        _isBuffed = false;
        _buffFireRateMult = 1f;
        _buffDamageMult = 1f;
        fireRate = _baseFireRate;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Legacy mouse pointer check
        try
        {
            if (Input.mousePresent && EventSystem.current.IsPointerOverGameObject())
                return true;

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    return true;
            }
        }
        catch (System.InvalidOperationException)
        {
            // Legacy input disabled
        }

#if ENABLE_INPUT_SYSTEM
        // New Input System mouse pointer check
        if (Mouse.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;
#endif

        return false;
    }
}
