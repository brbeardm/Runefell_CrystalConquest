using UnityEngine;

/// <summary>
/// Draws a bright X on the bridge directly at this object's transform.position.
/// Uses LineRenderer so it's visible in-game. Remove when done debugging.
/// </summary>
public class DebugGroundMarker : MonoBehaviour
{
    [SerializeField] private float size = 0.3f;
    [SerializeField] private Color color = Color.red;
    [SerializeField] private float lineWidth = 0.05f;

    private LineRenderer _line1;
    private LineRenderer _line2;

    private void Start()
    {
        _line1 = CreateLine("DebugX_Line1");
        _line2 = CreateLine("DebugX_Line2");
    }

    private void LateUpdate()
    {
        // Place X flat on the ground at transform.position
        Vector3 pos = transform.position;
        float half = size * 0.5f;

        // Line 1: top-left to bottom-right
        _line1.SetPosition(0, new Vector3(pos.x - half, pos.y + 0.01f, pos.z + half));
        _line1.SetPosition(1, new Vector3(pos.x + half, pos.y + 0.01f, pos.z - half));

        // Line 2: top-right to bottom-left
        _line2.SetPosition(0, new Vector3(pos.x + half, pos.y + 0.01f, pos.z + half));
        _line2.SetPosition(1, new Vector3(pos.x - half, pos.y + 0.01f, pos.z - half));
    }

    private LineRenderer CreateLine(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }
}
