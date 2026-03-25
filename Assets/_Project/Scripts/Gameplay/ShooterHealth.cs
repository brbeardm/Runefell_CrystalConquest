using UnityEngine;

public class ShooterHealth : MonoBehaviour
{
    private bool _isDead;

    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;

        if (other.CompareTag("Enemy"))
        {
            // Clones die instantly on enemy contact.
            // Main player also dies — this triggers game over via ShooterManager.
            Kill();
        }
    }

    public void Kill()
    {
        if (_isDead) return;
        _isDead = true;

        bool isMain = ShooterManager.Instance != null && ShooterManager.Instance.IsMainPlayer(gameObject);
        Debug.Log($"[ShooterHealth] Kill called on '{gameObject.name}' " +
                  $"(isMainPlayer={isMain}) at position {transform.position}");

        if (ShooterManager.Instance != null)
            ShooterManager.Instance.RemoveShooter(gameObject);

        Destroy(gameObject);
    }
}
