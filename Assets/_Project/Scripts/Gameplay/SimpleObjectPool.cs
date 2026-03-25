using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Very small, safe object pool. Instances are initially inactive.
/// Returned objects are set inactive and reparented to the pool root.
/// </summary>
public sealed class SimpleObjectPool
{
    private readonly GameObject _prefab;
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    private readonly Transform _root;

    public SimpleObjectPool(GameObject prefab, int initialSize, Transform optionalRoot = null)
    {
        _prefab = prefab;

        _root = new GameObject(prefab.name + "_Pool").transform;
        if (optionalRoot != null) _root.SetParent(optionalRoot, true);

        for (int i = 0; i < initialSize; i++)
        {
            var go = Object.Instantiate(_prefab, _root);
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }

    public GameObject Get()
    {
        GameObject go = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab, _root);
        // Caller will position/rotate; we activate here for convenience
        go.SetActive(true);
        return go;
    }

    public void ReturnToPool(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(_root, true);
        _pool.Enqueue(go);
    }
}
