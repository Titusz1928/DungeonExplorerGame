using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public static EnemySpawnManager Instance;

    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    [Header("Limits")]
    public int maxEnemies = 20;
    public float minSpawnDistanceFromPlayer = 30f;

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
        Log("TrySpawnEnemies tick");

        if (currentEnemyCount >= maxEnemies)
        {
            Log("Max enemies reached");
            return;
        }

        foreach (var zone in zones)
        {
            Log($"Checking zone {zone.name}");

            if (!zone.CanSpawnInZone())
            {
                Log($"Zone {zone.name} is full");
                continue;
            }

            Vector3 pos = zone.GetRandomPoint();
            Log($"Proposed spawn position {pos}");

            if (!CanSpawn(pos))
            {
                Log("CanSpawn() rejected position");
                continue;
            }

            int enemyIndex = zone.GetRandomEnemyIndex();
            Log($"Selected enemy prefab index {enemyIndex}");

            GameObject enemy = SpawnEnemy(enemyIndex, pos, zone.transform);

            if (enemy != null)
            {
                Log($"Spawned enemy {enemy.name}");
                zone.RegisterEnemy(enemy);
                return;
            }
            else
            {
                Log("SpawnEnemy returned null");
            }
        }

        Log("No valid zone found this tick");
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

