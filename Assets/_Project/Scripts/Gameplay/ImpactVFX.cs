using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns small crystal shard meshes on projectile impact.
/// Each impact type gets its own pre-colored material (SRP Batcher compatible).
/// </summary>
public class ImpactVFX : MonoBehaviour
{
    public static ImpactVFX Instance { get; private set; }

    [Header("Shard Settings")]
    [SerializeField] private int shardsPerHit = 6;
    [SerializeField] private float shardSpeed = 8f;
    [SerializeField] private float shardLifetime = 0.5f;
    [SerializeField] private float shardMinScale = 0.32f;
    [SerializeField] private float shardMaxScale = 0.8f;

    [Header("Colors")]
    [SerializeField] private Color enemyHitColor = new Color(1f, 0.3f, 0.1f, 1f);
    [SerializeField] private Color crystalHitColor = new Color(0.3f, 0.8f, 1f, 1f);
    [SerializeField] private Color heroCrystalHitColor = new Color(0.6f, 0.3f, 1f, 1f);

    [Header("Pool")]
    [SerializeField] private int poolSize = 50;

    // Separate pools per impact type so each has the right material baked in
    private readonly Dictionary<ImpactType, Queue<GameObject>> _pools = new();
    private readonly Dictionary<ImpactType, Material> _materials = new();
    private readonly List<ShardInstance> _active = new();
    private Mesh _shardMesh;

    private struct ShardInstance
    {
        public GameObject go;
        public Vector3 velocity;
        public float despawnTime;
        public float startScale;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Grab the cube mesh from a temporary primitive
        var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _shardMesh = temp.GetComponent<MeshFilter>().sharedMesh;
        Destroy(temp);

        // Create one material per impact type — set color directly on the material
        // so SRP Batcher has no issues
        CreateMaterial(ImpactType.Enemy, enemyHitColor);
        CreateMaterial(ImpactType.Crystal, crystalHitColor);
        CreateMaterial(ImpactType.HeroCrystal, heroCrystalHitColor);

        // Pre-warm pools
        foreach (ImpactType type in System.Enum.GetValues(typeof(ImpactType)))
        {
            _pools[type] = new Queue<GameObject>();
            int perType = poolSize / 3;
            for (int i = 0; i < perType; i++)
            {
                var shard = CreateShard(type);
                shard.SetActive(false);
                _pools[type].Enqueue(shard);
            }
        }
    }

    private void CreateMaterial(ImpactType type, Color color)
    {
        // Use the URP Lit shader from an existing renderer, or fall back
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader.name == "Hidden/InternalErrorShader")
        {
            // URP shader not found, use legacy
            mat = new Material(Shader.Find("Standard"));
        }

        // Make it emissive so shards glow and are always visible
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", color);
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
        }

        _materials[type] = mat;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var s = _active[i];

            if (Time.time >= s.despawnTime || s.go == null)
            {
                if (s.go != null)
                {
                    s.go.SetActive(false);
                    // Return to correct pool
                    foreach (var kvp in _pools)
                    {
                        // Just return to first available pool — they all share the same mesh
                        kvp.Value.Enqueue(s.go);
                        break;
                    }
                }
                _active.RemoveAt(i);
                continue;
            }

            s.velocity += Vector3.down * 14f * dt;
            s.go.transform.position += s.velocity * dt;
            s.go.transform.Rotate(500f * dt, 400f * dt, 300f * dt);

            float t = (s.despawnTime - Time.time) / shardLifetime;
            float scale = s.startScale * Mathf.Max(t, 0.1f);
            s.go.transform.localScale = Vector3.one * scale;

            _active[i] = s;
        }
    }

    public void SpawnImpact(Vector3 position, ImpactType type = ImpactType.Enemy, float hitSize = 1f)
    {
        // Scale shards proportional to the object hit (normalized around 1.0)
        float sizeMultiplier = Mathf.Clamp(hitSize / 2f, 0.5f, 3f);

        for (int i = 0; i < shardsPerHit; i++)
        {
            GameObject shard = GetShard(type);

            float scale = Random.Range(shardMinScale, shardMaxScale) * sizeMultiplier;
            shard.transform.position = position;
            shard.transform.localScale = Vector3.one * scale;
            shard.transform.rotation = Random.rotation;
            shard.SetActive(true);

            Vector3 dir = Random.onUnitSphere;
            dir.y = Mathf.Abs(dir.y) * 0.5f;
            float speed = shardSpeed * Random.Range(0.5f, 1.5f);

            _active.Add(new ShardInstance
            {
                go = shard,
                velocity = dir * speed,
                despawnTime = Time.time + shardLifetime * Random.Range(0.6f, 1f),
                startScale = scale
            });
        }
    }

    private GameObject GetShard(ImpactType type)
    {
        if (_pools.TryGetValue(type, out var pool) && pool.Count > 0)
        {
            var recycled = pool.Dequeue();
            // Ensure correct material
            var rend = recycled.GetComponent<MeshRenderer>();
            if (rend != null)
                rend.sharedMaterial = _materials[type];
            return recycled;
        }
        return CreateShard(type);
    }

    private GameObject CreateShard(ImpactType type)
    {
        var go = new GameObject("ImpactShard");
        go.transform.SetParent(transform);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = _shardMesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = _materials[type];
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return go;
    }

    public enum ImpactType
    {
        Enemy,
        Crystal,
        HeroCrystal
    }
}
