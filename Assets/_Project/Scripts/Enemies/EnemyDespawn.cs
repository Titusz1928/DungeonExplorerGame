using UnityEngine;

public class EnemyDespawn : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float despawnDistance = 120f; // Must be higher than SpawnManager's MaxSpawnDistance
    [SerializeField] private float checkInterval = 5f;     // Check every 5 seconds (saves CPU)

    private EnemyController controller;

    void Start()
    {
        controller = GetComponent<EnemyController>();
        // Start checking after a random delay so 30 enemies don't all check on the same frame
        InvokeRepeating(nameof(CheckDistance), Random.Range(1f, 3f), checkInterval);
    }

    void CheckDistance()
    {
        if (PlayerReference.PlayerTransform == null) return;

        // Don't despawn if the enemy is currently fighting the player!
        if (controller.getState() == EnemyState.Chasing || controller.getState() == EnemyState.Searching)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, PlayerReference.PlayerTransform.position);

        if (dist > despawnDistance)
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        if (controller.IsBoss)
        {
            // Bosses don't need to report back to the SpawnManager 
            // to be put in a "respawn pool". They just vanish, 
            // and the BossSpawnZone will recreate them when the player returns.
            Destroy(gameObject);
            return;
        }

        if (EnemySpawnManager.Instance != null)
        {
            EnemySpawnManager.Instance.HandleEnemyDespawn(controller);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, despawnDistance);
    }
}