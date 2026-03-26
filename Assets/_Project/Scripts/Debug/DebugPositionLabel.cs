using UnityEngine;

/// <summary>
/// Floats an XYZ position label above the GameObject in world space.
/// Each instance auto-staggers vertically so labels don't overlap.
/// Uses OnGUI — no Canvas needed. Remove when done debugging.
/// </summary>
public class DebugPositionLabel : MonoBehaviour
{
    [Tooltip("Base height above the object's pivot.")]
    [SerializeField] private float yOffset = 2.0f;

    [Tooltip("Label text color.")]
    [SerializeField] private Color color = Color.yellow;

    [Tooltip("Font size.")]
    [SerializeField] private int fontSize = 16;

    [Tooltip("Optional custom name. If empty, uses GameObject name.")]
    [SerializeField] private string labelName;

    private GUIStyle _style;
    private GUIStyle _bgStyle;
    private static int _instanceCounter;
    private int _myIndex;

    private void OnEnable()
    {
        _myIndex = _instanceCounter++;
        enabled = false; // Disable debug labels — re-enable in Inspector if needed
    }

    private void OnDisable()
    {
        _instanceCounter--;
    }

    private void OnGUI()
    {
        if (Camera.main == null) return;

        // Stagger each label higher so they don't overlap
        float stagger = _myIndex * 0.6f;
        Vector3 worldPos = transform.position + Vector3.up * (yOffset + stagger);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f) return;

        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            _bgStyle = new GUIStyle(GUI.skin.box);
        }

        _style.fontSize = fontSize;
        _style.normal.textColor = color;

        float guiY = Screen.height - screenPos.y;

        string name = string.IsNullOrEmpty(labelName) ? gameObject.name : labelName;
        string label = $"{name}\nX:{transform.position.x:F2}  Y:{transform.position.y:F2}  Z:{transform.position.z:F2}";
        Vector2 size = _style.CalcSize(new GUIContent(label));
        // CalcSize doesn't account for newlines well, so add height
        size.y *= 2.2f;
        size.x += 10f;

        Rect rect = new Rect(screenPos.x - size.x * 0.5f, guiY - size.y, size.x, size.y);

        // Dark background for readability
        GUI.Box(rect, GUIContent.none, _bgStyle);
        GUI.Label(rect, label, _style);
    }
}
