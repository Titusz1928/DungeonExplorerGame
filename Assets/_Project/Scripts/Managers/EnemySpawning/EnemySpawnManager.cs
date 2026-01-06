using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance;

    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    [Header("Limits")]
    public int maxEnemies = 30;
    public float minSpawnDistanceFromPlayer = 25f;
    public float maxSpawnDistanceToPlayer = 90;

    private int currentEnemyCount;

    public float spawnCheckInterval = 3f;

    private List<EnemySpawnZone> zones = new();


    public bool debugSpawning = false;

    // Keep track of which unique enemies are currently in the world
    private HashSet<string> activeInstanceIDs = new HashSet<string>();

    void Log(string msg)
    {
        if (debugSpawning)
            Debug.Log($"[EnemySpawnManager] {msg}");
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Log("EnemySpawnManager started, waiting for zones...");

        Log($"Found {zones.Count} spawn zones");

        foreach (var z in zones)
            Log($"Zone found: {z.name} at {z.transform.position}");

        InvokeRepeating(nameof(TrySpawnEnemies), 1f, spawnCheckInterval);
    }

    public bool CanSpawn(Vector3 position)
    {
        if (currentEnemyCount >= maxEnemies)
            return false;

        if (PlayerReference.PlayerTransform != null)
        {
            float dist = Vector3.Distance(
                PlayerReference.PlayerTransform.transform.position,
                position
            );

            if (dist < minSpawnDistanceFromPlayer)
                return false;
        }

        return true;
    }

    void TrySpawnEnemies()
    {
        if (currentEnemyCount >= maxEnemies) return;

        // 1. Create a copy of the zones list and shuffle it
        List<EnemySpawnZone> shuffledZones = new List<EnemySpawnZone>(zones);
        ShuffleList(shuffledZones);

        // 2. We can try to spawn more than one enemy per tick if we are way under the limit
        int spawnsNeeded = Mathf.Min(5, maxEnemies - currentEnemyCount);
        int spawnsCount = 0;

        foreach (var zone in shuffledZones)
        {
            if (spawnsCount >= spawnsNeeded) break;

            // NEW: Check if the zone is within a "Relevant Range"
            // No point spawning in a zone 500m away if the player is at 0,0,0
            if (PlayerReference.PlayerTransform != null)
            {
                float distToZone = Vector3.Distance(PlayerReference.PlayerTransform.position, zone.transform.position);
                if (distToZone > maxSpawnDistanceToPlayer + zone.radius) continue;
            }

            if (!zone.CanSpawnInZone()) continue;

            Vector3 pos = zone.GetRandomPoint();

            if (!CanSpawn(pos)) continue;

            int enemyIndex = zone.GetRandomEnemyIndex();
            GameObject enemy = SpawnEnemy(enemyIndex, pos, zone.transform);

            if (enemy != null)
            {
                zone.RegisterEnemy(enemy);
                spawnsCount++;
                Log($"Spawned {enemy.name} at {pos}. Total this tick: {spawnsCount}");
            }
        }
    }

    // Simple Fisher-Yates shuffle algorithm
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void RegisterZone(EnemySpawnZone zone)
    {
        if (!zones.Contains(zone))
        {
            zones.Add(zone);
            Log($"Registered spawn zone: {zone.name}");
        }
    }

    public void UnregisterZone(EnemySpawnZone zone)
    {
        zones.Remove(zone);
    }

    public GameObject SpawnEnemy(int prefabIndex, Vector3 position, Transform parent = null)
    {
        if (!CanSpawn(position))
            return null;

        GameObject enemy = Instantiate(
            enemyPrefabs[prefabIndex],
            position,
            Quaternion.identity,
            parent
        );

        currentEnemyCount++;

        EnemyController controller = enemy.GetComponent<EnemyController>();
        controller.OnEnemyDeath += HandleEnemyDeath;

        return enemy;
    }

    public void NotifyEnemyRemoved(EnemyController enemy)
    {
        currentEnemyCount = Mathf.Max(0, currentEnemyCount - 1);
        //Log($"Enemy removed. Current count: {currentEnemyCount}");
    }

    void HandleEnemyDeath(EnemyController enemy)
    {
        NotifyEnemyRemoved(enemy);
    }

    public void HandleEnemyDespawn(EnemyController enemy)
    {
        // 1. Clear from the unique ID tracker so it can be re-spawned/re-stored later
        if (!string.IsNullOrEmpty(enemy.instanceID))
        {
            activeInstanceIDs.Remove(enemy.instanceID);
        }

        // 2. Reduce the global count
        NotifyEnemyRemoved(enemy);

        Log($"Despawned {enemy.name}. Current total: {currentEnemyCount}");
    }

    public void RestoreEnemy(EnemySaveData data)
    {
        if (string.IsNullOrEmpty(data.instanceID)) return;
        if (activeInstanceIDs.Contains(data.instanceID)) return;

        GameObject prefab = enemyPrefabs.Find(p => p.GetComponent<EnemyController>().data.enemyID == data.enemyID);
        if (prefab == null) return;

        GameObject enemyGo = Instantiate(prefab, data.position, Quaternion.identity);
        EnemyController controller = enemyGo.GetComponent<EnemyController>();

        // 1. Inject saved ID
        controller.instanceID = data.instanceID;
        activeInstanceIDs.Add(controller.instanceID);

        // 2. Restore Combat Stats via the new EnemyStats component
        EnemyStats stats = enemyGo.GetComponent<EnemyStats>();
        if (stats == null) stats = enemyGo.AddComponent<EnemyStats>();

        // We manually initialize with the controller and set the saved HP
        stats.Initialize(controller);
        stats.currentHP = data.currentHP;

        // 3. Restore AI State
        controller.SetState(data.currentState);
        controller.SetGuardCenter(data.guardCenter);

        // 4. Register events
        currentEnemyCount++;
        controller.OnEnemyDeath += (e) => {
            activeInstanceIDs.Remove(e.instanceID);
            HandleEnemyDeath(e);
        };
    }
}

