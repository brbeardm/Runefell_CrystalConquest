using UnityEngine;

/// <summary>
/// Drives the player Animator based on movement direction.
/// Feeds MoveX/MoveZ into a 2D blend tree for directional locomotion
/// (walk forward, backward, strafe left/right).
/// </summary>
public class PlayerAnimationDriver : MonoBehaviour
{
    [Tooltip("Animator on the player model child. Auto-found if left empty.")]
    [SerializeField] private Animator animator;

    [Tooltip("Minimum speed (units/sec) to count as moving.")]
    [SerializeField] private float moveThreshold = 0.05f;

    [Tooltip("How quickly blend tree values respond to direction changes.")]
    [SerializeField] private float blendSmoothing = 10f;

    [Tooltip("The movement speed (units/sec) at which the walk animation plays at 1x. Increase if the player skates.")]
    [SerializeField] private float animBaseSpeed = 2f;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveZ = Animator.StringToHash("MoveZ");

    private Vector3 _lastPosition;
    private float _smoothX;
    private float _smoothZ;
    private float _debugTimer;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _lastPosition = transform.position;

        if (animator != null)
        {
            // If controller reference is broken (deleted/recreated GUID), load by name
            if (animator.runtimeAnimatorController == null)
            {
                var ctrl = Resources.Load<RuntimeAnimatorController>("PlayerController");
                if (ctrl != null)
                {
                    animator.runtimeAnimatorController = ctrl;
                    Debug.Log($"[PlayerAnimDriver] Loaded PlayerController from Resources!");
                }
                else
                {
                    Debug.LogError("[PlayerAnimDriver] Animator has NO controller and could not load from Resources! Place PlayerController.controller in a Resources folder.");
                }
            }

            Debug.Log($"[PlayerAnimDriver] Animator found on '{animator.gameObject.name}', controller='{animator.runtimeAnimatorController?.name}', enabled={animator.enabled}");
            foreach (var p in animator.parameters)
                Debug.Log($"[PlayerAnimDriver] Param: {p.name} ({p.type})");
        }
        else
        {
            Debug.LogError("[PlayerAnimDriver] No Animator found in children!");
        }
    }

    private void LateUpdate()
    {
        if (animator == null) return;

        Vector3 delta = transform.position - _lastPosition;
        float speed = delta.magnitude / Time.deltaTime;
        _lastPosition = transform.position;

        bool moving = speed > moveThreshold;
        animator.SetBool(IsMoving, moving);

        _debugTimer += Time.deltaTime;
        if (_debugTimer > 2f)
        {
            _debugTimer = 0f;
            var info = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[PlayerAnimDriver] moving={moving}, speed={speed:F2}, MoveX={_smoothX:F2}, MoveZ={_smoothZ:F2}, animState={info.shortNameHash}, animSpeed={animator.speed}, inTransition={animator.IsInTransition(0)}, normalizedTime={info.normalizedTime:F2}");
        }

        if (moving)
        {
            // Normalize direction so blend tree gets -1..1 range
            Vector3 dir = delta.normalized;
            _smoothX = Mathf.Lerp(_smoothX, dir.x, 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime));
            _smoothZ = Mathf.Lerp(_smoothZ, dir.z, 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime));
        }
        else
        {
            // Decay to zero when stopped
            _smoothX = Mathf.Lerp(_smoothX, 0f, 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime));
            _smoothZ = Mathf.Lerp(_smoothZ, 0f, 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime));
        }

        animator.SetFloat(MoveX, _smoothX);
        animator.SetFloat(MoveZ, _smoothZ);

        // Scale animation playback to match actual movement speed
        // so feet don't slide. Clamp to avoid frozen or absurdly fast anims.
        if (moving)
            animator.speed = Mathf.Clamp(speed / animBaseSpeed, 0.5f, 3f);
        else
            animator.speed = 1f;
    }
}
