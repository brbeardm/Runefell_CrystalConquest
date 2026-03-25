using UnityEngine;
using System.Collections;

public class CrystalBallSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject crystalBallPrefab;
    [SerializeField] private CrystalBallData data;

    [Header("Spawning")]
    [Tooltip("Seconds between crystal ball spawns.")]
    [SerializeField] private float spawnInterval = 4f;

    [Header("Spawn X Position (Lane 10)")]
    [Tooltip("Fixed X position for crystal ball lane.")]
    [SerializeField] private float spawnX = 3.6f;

    [Header("Spawn Height")]
    [Tooltip("Y position for crystal balls. Match this to your projectile FirePoint Y so they collide.")]
    [SerializeField] private float spawnY = 0.5f;

    [Header("Pooling")]
    [SerializeField] private int poolSize = 5;

    private SimpleObjectPool _pool;
    private bool _spawning;

    private void OnEnable()
    {
        GameManager.OnGameStarted += HandleGameStarted;
        GameManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= HandleGameStarted;
        GameManager.OnGameOver -= HandleGameOver;
    }

    private void Start()
    {
        if (crystalBallPrefab != null)
            _pool = new SimpleObjectPool(crystalBallPrefab, poolSize, transform);

        StartSpawning();
    }

    private void HandleGameStarted()
    {
        StartSpawning();
    }

    private void HandleGameOver()
    {
        StopAllCoroutines();
        _spawning = false;
    }

    private void StartSpawning()
    {
        if (_spawning) return;
        _spawning = true;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (GameManager.Instance != null && GameManager.Instance.State == GameManager.GameState.GameOver)
                break;

            SpawnCrystalBall();
        }
    }

    private void SpawnCrystalBall()
    {
        GameObject go = _pool != null ? _pool.Get() : Instantiate(crystalBallPrefab);

        go.transform.SetParent(null, true);
        go.transform.position = new Vector3(spawnX, spawnY, transform.position.z);
        go.SetActive(true);

        var ball = go.GetComponent<CrystalBall>();
        if (ball != null)
        {
            ball.Initialize(data, _pool != null ? _pool.ReturnToPool : null);
        }
    }
}
