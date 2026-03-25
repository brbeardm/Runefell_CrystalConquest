using UnityEngine;

public class CheckSleep : MonoBehaviour
{
    void Update()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Player is sleeping: {rb.IsSleeping()}");
        }
    }
}
