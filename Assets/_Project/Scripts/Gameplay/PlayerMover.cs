using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Relative-drag movement for mobile (and desktop testing).
/// Touch/click anywhere → that becomes the drag origin.
/// Drag from origin → player moves by the same delta.
/// Lift → origin resets. Next touch starts a new origin.
/// Player Y stays constant (flat bridge).
/// </summary>
public class PlayerMover : MonoBehaviour
{
    [Header("Bridge Bounds (world units)")]
    [SerializeField] private float minX = -6f;
    [SerializeField] private float maxX = 6f;
    [SerializeField] private float minZ = -6f;
    [SerializeField] private float maxZ = -4f;

    [Header("Movement Feel")]
    [Tooltip("Multiplier for drag-to-movement. 1 = finger moves 1 world unit, player moves 1 unit.")]
    [SerializeField] private float dragSensitivity = 1f;

    [Tooltip("How quickly the player catches up to the target. Higher = snappier.")]
    [SerializeField] private float followSpeed = 25f;

    [Tooltip("Keyboard movement speed (for desktop testing).")]
    [SerializeField] private float keyboardSpeed = 5f;

    private Camera _cam;
    private Rigidbody _rb;
    private float _playerY;
    private float _targetX;
    private float _targetZ;

    // Drag state
    private bool _isDragging;
    private int _activeTouchId = -1;
    private Vector3 _dragStartWorld;   // world position where drag began
    private float _originX;            // player X when drag began
    private float _originZ;            // player Z when drag began

    private void Awake()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody>();

        // PlayerMover owns all movement via transform.position.
        // Make the Rigidbody kinematic so the physics engine never
        // applies depenetration forces that sink the player into the bridge.
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Start()
    {
        // Capture Y in Start (not Awake) so parent transforms have been
        // fully applied first. Awake fires before the scene hierarchy is
        // settled, so transform.position.y can read 0 instead of 0.25.
        _playerY = transform.position.y;
        _targetX = transform.position.x;
        _targetZ = transform.position.z;
    }

    private void Update()
    {
        HandleTouch();
        HandleMouse();
        HandleKeyboard();

        // Clamp to bridge bounds
        _targetX = Mathf.Clamp(_targetX, minX, maxX);
        _targetZ = Mathf.Clamp(_targetZ, minZ, maxZ);

        // Lock Y strictly — never allow anything to change it
        float lockedY = _playerY;

        // Move on X/Z only, Y is always _playerY
        Vector3 current = transform.position;
        float newX = Mathf.Lerp(current.x, _targetX, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        float newZ = Mathf.Lerp(current.z, _targetZ, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.position = new Vector3(newX, lockedY, newZ);
    }

    // ─── Touch (mobile) ─────────────────────────────────────────────

    private void HandleTouch()
    {
#if ENABLE_INPUT_SYSTEM
        var touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            var touches = touchscreen.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var t = touches[i];
                var phase = t.phase.ReadValue();
                int id = t.touchId.ReadValue();

                if (phase == UnityEngine.InputSystem.TouchPhase.Began && !_isDragging)
                {
                    BeginDrag(t.position.ReadValue());
                    _activeTouchId = id;
                    return;
                }

                if (_isDragging && id == _activeTouchId)
                {
                    if (phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                        phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                    {
                        UpdateDrag(t.position.ReadValue());
                        return;
                    }

                    if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                        phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                    {
                        EndDrag();
                        return;
                    }
                }
            }
        }
#endif

        // Legacy touch
        try
        {
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);

                    if (t.phase == UnityEngine.TouchPhase.Began && !_isDragging)
                    {
                        BeginDrag(t.position);
                        _activeTouchId = t.fingerId;
                        return;
                    }

                    if (_isDragging && t.fingerId == _activeTouchId)
                    {
                        if (t.phase == UnityEngine.TouchPhase.Moved ||
                            t.phase == UnityEngine.TouchPhase.Stationary)
                        {
                            UpdateDrag(t.position);
                            return;
                        }

                        if (t.phase == UnityEngine.TouchPhase.Ended ||
                            t.phase == UnityEngine.TouchPhase.Canceled)
                        {
                            EndDrag();
                            return;
                        }
                    }
                }
            }
        }
        catch (System.InvalidOperationException) { }
    }

    // ─── Mouse (desktop testing) ────────────────────────────────────

    private void HandleMouse()
    {
        // Don't double-handle if touch is active
        if (_isDragging && _activeTouchId >= 0) return;

        bool pressed = false;
        bool justPressed = false;
        bool justReleased = false;
        Vector2 mousePos = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            pressed = Mouse.current.leftButton.isPressed;
            justPressed = Mouse.current.leftButton.wasPressedThisFrame;
            justReleased = Mouse.current.leftButton.wasReleasedThisFrame;
            mousePos = Mouse.current.position.ReadValue();
        }
#endif

        try
        {
            if (!pressed && Input.GetMouseButton(0))
            {
                pressed = true;
                mousePos = Input.mousePosition;
            }
            if (!justPressed && Input.GetMouseButtonDown(0))
            {
                justPressed = true;
                mousePos = Input.mousePosition;
            }
            if (!justReleased && Input.GetMouseButtonUp(0))
                justReleased = true;
        }
        catch (System.InvalidOperationException) { }

        if (justPressed && !_isDragging)
        {
            BeginDrag(mousePos);
            _activeTouchId = -1; // mouse, not touch
        }
        else if (pressed && _isDragging && _activeTouchId == -1)
        {
            UpdateDrag(mousePos);
        }
        else if (justReleased && _isDragging && _activeTouchId == -1)
        {
            EndDrag();
        }
    }

    // ─── Keyboard (desktop testing) ─────────────────────────────────

    private void HandleKeyboard()
    {
        float xInput = 0f;
        float zInput = 0f;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) xInput += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) xInput -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) zInput += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) zInput -= 1f;
        }
#endif

        try
        {
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) xInput += 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) xInput -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) zInput += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) zInput -= 1f;
        }
        catch (System.InvalidOperationException) { }

        _targetX += xInput * keyboardSpeed * Time.deltaTime;
        _targetZ += zInput * keyboardSpeed * Time.deltaTime;
    }

    // ─── Drag helpers ───────────────────────────────────────────────

    private void BeginDrag(Vector2 screenPos)
    {
        _isDragging = true;
        _dragStartWorld = ScreenToWorldFlat(screenPos);
        _originX = _targetX;
        _originZ = _targetZ;
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        Vector3 currentWorld = ScreenToWorldFlat(screenPos);
        Vector3 delta = currentWorld - _dragStartWorld;
        _targetX = _originX + delta.x * dragSensitivity;
        _targetZ = _originZ + delta.z * dragSensitivity;
    }

    private void EndDrag()
    {
        _isDragging = false;
        _activeTouchId = -1;
    }

    /// <summary>
    /// Projects a screen point onto the flat bridge plane (Y = _playerY).
    /// </summary>
    private Vector3 ScreenToWorldFlat(Vector2 screenPos)
    {
        Ray ray = _cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, _playerY, 0f));

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return Vector3.zero;
    }
}
