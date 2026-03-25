using UnityEngine;
using UnityEditor;

public class GenerateCrystalMesh
{
    [MenuItem("Tools/Generate Crystal Mesh")]
    public static void Generate()
    {
        Mesh mesh = new Mesh();
        mesh.name = "CrystalShard";

        // 6 vertices for a diamond shape (octahedron)
        Vector3 vFront = new Vector3(0, 0, 0.5f);
        Vector3 vBack = new Vector3(0, 0, -0.5f);
        
        float midZ = -0.2f;
        float width = 0.5f;
        Vector3 vTop = new Vector3(0, width, midZ);
        Vector3 vBottom = new Vector3(0, -width, midZ);
        Vector3 vRight = new Vector3(width, 0, midZ);
        Vector3 vLeft = new Vector3(-width, 0, midZ);

        Vector3[] vertices = new Vector3[]
        {
            vFront, vBack, vTop, vBottom, vRight, vLeft
        };

        // Triangles (Clockwise winding)
        int[] triangles = new int[]
        {
            // Front half
            0, 4, 2, // Front-Right-Top
            0, 3, 4, // Front-Bottom-Right
            0, 5, 3, // Front-Left-Bottom
            0, 2, 5, // Front-Top-Left

            // Back half
            1, 2, 4, // Back-Top-Right
            1, 4, 3, // Back-Right-Bottom
            1, 3, 5, // Back-Bottom-Left
            1, 5, 2  // Back-Left-Top
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        AssetDatabase.CreateAsset(mesh, "Assets/_Project/Prefabs/Shooters/CrystalShardMesh.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Crystal mesh generated!");
    }
}
