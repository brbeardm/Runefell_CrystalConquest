using UnityEngine;
using System;
using System.Collections.Generic;

public class ShooterManager : MonoBehaviour
{
    public static ShooterManager Instance { get; private set; }

    public static event Action<int> OnShooterCountChanged;

    [Header("Clone Shooter")]
    [SerializeField] private GameObject cloneShooterPrefab;

    [Header("Hero Shooter")]
    [SerializeField] private GameObject heroShooterPrefab;

    [Header("Shoulder-to-Shoulder Formation")]
    [Tooltip("Horizontal spacing between shooters (center-to-center). Tune in Inspector so models touch.")]
    [SerializeField] private float cloneXSpacing = 0.25f;

    [Tooltip("How far behind the player clones line up (negative = behind).")]
    [SerializeField] private float cloneZOffset = -0.2f;

    private readonly List<GameObject> _shooters = new List<GameObject>();
    private readonly List<GameObject> _clones = new List<GameObject>();
    private GameObject _heroShooter;
    private Transform _playerTransform;
    private GameObject _playerObject;

    public int ShooterCount => _shooters.Count;
    public int CloneCount => _clones.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        var player = FindAnyObjectByType<PlayerShooter>();
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerObject = player.gameObject;
        }
        Debug.Log($"[ShooterManager] cloneXSpacing = {cloneXSpacing} — if this is not 0.15, update it in the Inspector!");
    }

    /// <summary>
    /// Returns true if the given GameObject is the main player (not a clone).
    /// </summary>
    public bool IsMainPlayer(GameObject obj)
    {
        return obj != null && obj == _playerObject;
    }

    public void RegisterShooter(GameObject shooter)
    {
        if (shooter == null || _shooters.Contains(shooter)) return;
        _shooters.Add(shooter);
        OnShooterCountChanged?.Invoke(_shooters.Count);
    }

    public void RemoveShooter(GameObject shooter)
    {
        if (shooter == null) return;

        bool isClone = _clones.Contains(shooter);

        _shooters.Remove(shooter);
        _clones.Remove(shooter);
        OnShooterCountChanged?.Invoke(_shooters.Count);

        // Reposition surviving clones to fill gaps
        RepositionClones();

        // Only end game if the MAIN PLAYER is destroyed — never for clones
        if (!isClone && shooter == _playerObject && GameManager.Instance != null)
        {
            Debug.Log("[ShooterManager] Main player destroyed — Game Over");
            GameManager.Instance.EndGame();
        }
        else if (isClone)
        {
            Debug.Log($"[ShooterManager] Clone destroyed. {_clones.Count} clones remaining.");
        }
    }

    public void AddCloneShooter()
    {
        if (cloneShooterPrefab == null)
        {
            Debug.LogWarning("ShooterManager: No cloneShooterPrefab assigned.");
            return;
        }

        if (_playerTransform == null)
        {
            var player = FindAnyObjectByType<PlayerShooter>();
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerObject = player.gameObject;
            }
        }

        Vector3 spawnPos = _playerTransform != null
            ? _playerTransform.position
            : Vector3.zero;

        GameObject clone = Instantiate(cloneShooterPrefab, spawnPos, Quaternion.identity);
        _clones.Add(clone);

        var follower = clone.GetComponent<CloneFollower>();
        if (follower == null)
            follower = clone.AddComponent<CloneFollower>();

        follower.target = _playerTransform;

        // Shoulder-to-shoulder, slightly behind player
        var offset = CalculateLineOffset(_clones.Count);
        follower.xOffset = offset.x;
        follower.zOffset = cloneZOffset;

        Debug.Log($"[ShooterManager] Clone #{_clones.Count} spawned. X offset={offset.x:F2}");

        RegisterShooter(clone);
    }

    public void AddHeroShooter()
    {
        if (heroShooterPrefab == null)
        {
            Debug.LogWarning("ShooterManager: No heroShooterPrefab assigned.");
            return;
        }

        if (_heroShooter != null) return;

        Vector3 pos = _playerTransform != null
            ? _playerTransform.position
            : Vector3.zero;

        _heroShooter = Instantiate(heroShooterPrefab, pos, Quaternion.identity);

        var follower = _heroShooter.GetComponent<CloneFollower>();
        if (follower == null)
            follower = _heroShooter.AddComponent<CloneFollower>();

        follower.target = _playerTransform;
        // Hero takes the first clone slot (right of player), push existing clones out
        follower.xOffset = cloneXSpacing;
        follower.zOffset = cloneZOffset;

        RegisterShooter(_heroShooter);
    }

    public void RemoveHeroShooter()
    {
        if (_heroShooter == null) return;

        RemoveShooter(_heroShooter);
        Destroy(_heroShooter);
        _heroShooter = null;
    }

    /// <summary>
    /// Shoulder-to-shoulder line formation. Clones alternate left/right of the player.
    ///
    /// Layout (P = player, clones numbered by spawn order):
    ///   ... 4  2  P  1  3  5 ...
    ///
    /// Clone 1 = right, 2 = left, 3 = further right, etc.
    /// All on the same Z as the player (z offset = 0).
    /// </summary>
    private Vector3 CalculateLineOffset(int cloneNumber)
    {
        // Slot: 1 → +1, 2 → -1, 3 → +2, 4 → -2, ...
        int slot = (cloneNumber + 1) / 2; // 1,1,2,2,3,3,...
        float sign = (cloneNumber % 2 == 1) ? 1f : -1f;
        float x = sign * slot * cloneXSpacing;
        return new Vector3(x, 0f, 0f);
    }

    private void RepositionClones()
    {
        for (int i = 0; i < _clones.Count; i++)
        {
            var follower = _clones[i].GetComponent<CloneFollower>();
            if (follower != null)
            {
                var offset = CalculateLineOffset(i + 1);
                follower.xOffset = offset.x;
            }
        }
    }
}
