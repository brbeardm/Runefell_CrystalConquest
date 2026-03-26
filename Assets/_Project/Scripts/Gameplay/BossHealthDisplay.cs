using UnityEngine;

/// <summary>
/// Renders a health counter above the boss using OnGUI (screen-space).
/// Attach to boss prefabs alongside Enemy and BossBehavior.
/// </summary>
public class BossHealthDisplay : MonoBehaviour
{
    [SerializeField] private float yOffset = 4f;
    [SerializeField] private Color textColor = Color.red;
    [SerializeField] private int fontSize = 40;

    private Enemy _enemy;
    private GUIStyle _style;
    private GUIStyle _bgStyle;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    private void OnGUI()
    {
        if (_enemy == null || _enemy.IsDead || Camera.main == null) return;

        Vector3 worldPos = transform.position + Vector3.up * yOffset;
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
        _style.normal.textColor = textColor;

        float guiY = Screen.height - screenPos.y;
        string label = $"{_enemy.CurrentHealth}";
        Vector2 size = _style.CalcSize(new GUIContent(label));
        size.x += 10f;
        size.y += 4f;

        Rect rect = new Rect(screenPos.x - size.x * 0.5f, guiY - size.y, size.x, size.y);
        GUI.Box(rect, GUIContent.none, _bgStyle);
        GUI.Label(rect, label, _style);
    }
}
