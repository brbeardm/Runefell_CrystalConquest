using UnityEngine;

/// <summary>
/// Drives the player Animator based on movement velocity.
/// Attach to the Player GameObject (same object as PlayerMover).
/// Reads position delta each frame and sets IsMoving on the Animator.
/// </summary>
public class PlayerAnimationDriver : MonoBehaviour
{
    [Tooltip("Animator on the player model child. Auto-found if left empty.")]
    [SerializeField] private Animator animator;

    [Tooltip("Minimum speed (units/sec) to count as moving.")]
    [SerializeField] private float moveThreshold = 0.05f;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsShooting = Animator.StringToHash("IsShooting");

    private Vector3 _lastPosition;
    private bool _isShooting;

    private void OnEnable()
    {
        PlayerShooter.OnPlayerFired += HandlePlayerFired;
    }

    private void OnDisable()
    {
        PlayerShooter.OnPlayerFired -= HandlePlayerFired;
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (animator == null) return;

        Vector3 delta = transform.position - _lastPosition;
        float speed = delta.magnitude / Time.deltaTime;
        _lastPosition = transform.position;

        bool moving = speed > moveThreshold;
        animator.SetBool(IsMoving, moving);
        animator.SetFloat(Speed, speed);
        animator.SetBool(IsShooting, _isShooting);

        // Reset shooting flag each frame — it gets set again next fire event
        _isShooting = false;

        // Pause the base layer when standing still so the model holds its pose
        // rather than walking in place. Resume at full speed when moving.
        animator.speed = moving ? 1f : 0f;
    }

    private void HandlePlayerFired()
    {
        _isShooting = true;
        // Keep animator running during shooting even if standing still
        if (animator != null) animator.speed = 1f;
    }
}
