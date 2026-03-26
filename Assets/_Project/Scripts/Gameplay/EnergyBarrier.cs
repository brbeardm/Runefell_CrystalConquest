using UnityEngine;

/// <summary>
/// Three-sided energy barrier enclosure around the HeroCrystal zone.
/// Blocks enemies and projectiles via physics colliders (NOT triggers).
/// Open side faces the player (negative Z / front of bridge).
/// Visual: semi-transparent emissive walls.
/// </summary>
public class EnergyBarrier : MonoBehaviour
{
    [Header("Barrier Dimensions")]
    [Tooltip("Left edge X of the barrier zone.")]
    [SerializeField] private float minX = -4.0f;
    [Tooltip("Right edge X of the barrier zone.")]
    [SerializeField] private float maxX = -2.4f;
    [Tooltip("Back Z of the barrier (behind crystal).")]
    [SerializeField] private float backZ = -1.5f;
    [Tooltip("Front Z of the barrier (toward enemies). Open side faces player at negative Z.")]
    [SerializeField] private float frontZ = -4.5f;

    [Header("Visuals")]
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float wallThickness = 0.1f;
    [SerializeField] private Color barrierColor = new Color(0.2f, 0.6f, 1f, 0.3f);

    private void Start()
    {
        CreateWalls();
    }

    private void CreateWalls()
    {
        float zDepth = frontZ - backZ; // negative value (front is more negative than back)
        float zCenter = (frontZ + backZ) / 2f;
        float xWidth = maxX - minX;
        float xCenter = (minX + maxX) / 2f;
        float yCenter = wallHeight / 2f;

        // Left wall (along Z axis at minX)
        CreateWall("BarrierLeft",
            new Vector3(minX - wallThickness / 2f, yCenter, zCenter),
            new Vector3(wallThickness, wallHeight, Mathf.Abs(zDepth)));

        // Right wall (along Z axis at maxX)
        CreateWall("BarrierRight",
            new Vector3(maxX + wallThickness / 2f, yCenter, zCenter),
            new Vector3(wallThickness, wallHeight, Mathf.Abs(zDepth)));

        // Back wall (along X axis at backZ)
        CreateWall("BarrierBack",
            new Vector3(xCenter, yCenter, backZ + wallThickness / 2f),
            new Vector3(xWidth + wallThickness * 2f, wallHeight, wallThickness));
    }

    private void CreateWall(string wallName, Vector3 localPos, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = localPos;
        wall.transform.localRotation = Quaternion.identity;

        // Box collider — NOT a trigger, blocks physics
        var col = wall.AddComponent<BoxCollider>();
        col.size = size;
        col.isTrigger = false;

        // Visual mesh
        var meshFilter = wall.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateBoxMesh(size);

        var meshRenderer = wall.AddComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        // Semi-transparent
        if (mat.HasProperty("_Surface"))
        {
            mat.SetFloat("_Surface", 1f); // Transparent
            mat.SetFloat("_Blend", 0f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
        }

        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", barrierColor);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", barrierColor * 1.5f);
        }

        meshRenderer.material = mat;
    }

    private Mesh CreateBoxMesh(Vector3 size)
    {
        // Simple unit cube scaled by size
        var mesh = new Mesh();
        float x = size.x / 2f, y = size.y / 2f, z = size.z / 2f;

        mesh.vertices = new Vector3[]
        {
            // Front
            new(-x, -y, -z), new(x, -y, -z), new(x, y, -z), new(-x, y, -z),
            // Back
            new(x, -y, z), new(-x, -y, z), new(-x, y, z), new(x, y, z),
            // Top
            new(-x, y, -z), new(x, y, -z), new(x, y, z), new(-x, y, z),
            // Bottom
            new(-x, -y, z), new(x, -y, z), new(x, -y, -z), new(-x, -y, -z),
            // Left
            new(-x, -y, z), new(-x, -y, -z), new(-x, y, -z), new(-x, y, z),
            // Right
            new(x, -y, -z), new(x, -y, z), new(x, y, z), new(x, y, -z),
        };

        mesh.triangles = new int[]
        {
            0,2,1, 0,3,2,
            4,6,5, 4,7,6,
            8,10,9, 8,11,10,
            12,14,13, 12,15,14,
            16,18,17, 16,19,18,
            20,22,21, 20,23,22,
        };

        mesh.RecalculateNormals();
        return mesh;
    }
}
