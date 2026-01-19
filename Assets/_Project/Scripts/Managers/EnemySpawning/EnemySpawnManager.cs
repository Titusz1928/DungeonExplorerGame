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

        List<EnemySpawnZone> priorityZones = zones.FindAll(z => (z is BossSpawnZone b && b.isPriority));
        List<EnemySpawnZone> normalZones = zones.FindAll(z => !(z is BossSpawnZone b && b.isPriority));

        // DEBUG: See how many boss zones we actually have
        if (priorityZones.Count > 0)
        {
            Debug.Log($"[SPAWN DEBUG] Found {priorityZones.Count} Boss Zones in the registry.");
            foreach (var bz in priorityZones)
            {
                Debug.Log($"[SPAWN DEBUG] Boss Zone at: {bz.transform.position} | Name: {bz.gameObject.name}");
            }
        }

        ShuffleList(normalZones);
        List<EnemySpawnZone> finalOrder = new List<EnemySpawnZone>();
        finalOrder.AddRange(priorityZones);
        finalOrder.AddRange(normalZones);

        int spawnsNeeded = Mathf.Min(5, maxEnemies - currentEnemyCount);
        int spawnsCount = 0;

        foreach (var zone in finalOrder)
        {
            if (spawnsCount >= spawnsNeeded) break;

            if (PlayerReference.PlayerTransform != null)
            {
                float distToZone = Vector3.Distance(PlayerReference.PlayerTransform.position, zone.transform.position);
                float maxDist = (zone is BossSpawnZone) ? maxSpawnDistanceToPlayer * 1.5f : maxSpawnDistanceToPlayer;

                if (distToZone > maxDist + zone.radius) continue;
                if (distToZone < minSpawnDistanceFromPlayer) continue;
            }

            // DEBUG: See if the zone is rejecting the spawn
            if (!zone.CanSpawnInZone())
            {
                if (zone is BossSpawnZone) Debug.Log($"[SPAWN DEBUG] Boss Zone at {zone.transform.position} blocked spawn (CanSpawnInZone was false).");
                continue;
            }

            Vector3 pos = zone.GetRandomPoint();
            if (!CanSpawn(pos)) continue;

            int enemyIndex = zone.GetRandomEnemyIndex();
            GameObject enemy = SpawnEnemy(enemyIndex, pos, null);

            if (enemy != null)
            {
                string zoneType = zone is BossSpawnZone ? "BOSS ZONE" : "REGULAR ZONE";
                Debug.Log($"[SPAWN DEBUG] Successfully spawned {enemy.name} from {zoneType} at {zone.transform.position}");

                if (zone is BossSpawnZone bossZone)
                {
                    bossZone.OnSuccessfulSpawn(enemy);
                }
                else
                {
                    // If a regular zone just spawned a boss, we need to know!
                    if (enemy.GetComponent<EnemyController>().IsBoss)
                    {
                        Debug.LogError($"[CRITICAL] Regular SpawnZone at {zone.transform.position} is spawning a BOSS! Check its allowedEnemyIndices.");
                    }
                    zone.RegisterEnemy(enemy);
                }
                spawnsCount++;
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
        controller.OnEnemyDeath += (e) =>
        {
            activeInstanceIDs.Remove(e.instanceID);
            HandleEnemyDeath(e);
        };
    }

    public GameObject SpawnBossFromZone(int enemyIndex, Vector3 position, BossSpawnZone zone)
    {
        if (enemyIndex < 0 || enemyIndex >= enemyPrefabs.Count) return null;

        GameObject boss = Instantiate(enemyPrefabs[enemyIndex], position, Quaternion.identity);
        EnemyController controller = boss.GetComponent<EnemyController>();
   

        // Assign a unique but consistent ID for the session
        controller.instanceID = $"BOSS_{zone.transform.position.x}_{zone.transform.position.y}";

        return boss;
    }
}

