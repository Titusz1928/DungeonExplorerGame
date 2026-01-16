using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public struct StructureSpawnInfo
{
    public GameObject prefab;
    [Range(0, 100)] public float weight; // Higher weight = more common
}

public class WorldObjectSpawner : MonoBehaviour
{
    [Header("References")]
    public TileTerrainGenerator terrainGenerator;

    [Header("Prefabs")]
    public GameObject treePrefab;
    public GameObject[] bushPrefabs;
    public GameObject[] chestPrefabs;
    public GameObject[] enemySpawnPrefabs;

    public StructureSpawnInfo[] structures;

    [Header("Spawn Settings")]
    public float treeChance = 0.02f;
    public float bushChance = 0.03f;
    public float chestChance = 0.005f;
    public float enemyZoneChance = 0.002f;
    public float houseChance = 0.000005f;

    public int worldSize = 1280;


    // Lookup for prefab by name
    private Dictionary<string, GameObject> prefabLookup;

    void Awake()
    {
        EnsureReferences();

        prefabLookup = new Dictionary<string, GameObject>();
        if (treePrefab != null) prefabLookup[treePrefab.name] = treePrefab;
        foreach (var p in bushPrefabs) prefabLookup[p.name] = p;
        foreach (var p in chestPrefabs) prefabLookup[p.name] = p;
        foreach (var p in enemySpawnPrefabs) prefabLookup[p.name] = p;

        // Populate structures into lookup
        foreach (var s in structures)
        {
            if (s.prefab != null) prefabLookup[s.prefab.name] = s.prefab;
        }
    }

    private void EnsureReferences()
    {
        // If the generator is missing, find the persistent one
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<TileTerrainGenerator>();
        }
    }

    public void SpawnObjectsInChunk(Vector2Int chunkCoord, int chunkSize, Transform chunkParent)
    {
        // Check if we already have saved chunk data
        if (WorldSaveData.Instance.HasChunkData(chunkCoord))
        {
            Debug.Log("[OBJECT SPAWNER] restoring old chunk:" + chunkCoord);

            ChunkData data = WorldSaveData.Instance.GetChunkData(chunkCoord);
            SpawnFromChunkData(data, chunkParent);
            return;
        }

        // Otherwise, generate chunk for the first time
        Debug.Log("[OBJECT SPAWNER] creating new chunk:" + chunkCoord);

        ChunkData newData = new ChunkData() { chunkCoord = chunkCoord };

        Random.InitState(chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663 ^ GameSettings.Instance.seed);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;

                float height = terrainGenerator.GetHeight(worldX, worldY, worldSize);
                if (height < terrainGenerator.sandyGrassLevel)
                    continue;

                Vector3 pos = new Vector3(worldX + 0.5f, worldY + 0.5f, 0f);

                TrySpawn(treePrefab, treeChance, pos, chunkParent, newData);
                TrySpawnRandom(bushPrefabs, bushChance, chunkCoord, new Vector2Int(x, y), chunkParent, newData);
                TrySpawnRandom(chestPrefabs, chestChance, chunkCoord, new Vector2Int(x, y), chunkParent, newData);
                TrySpawnRandom(enemySpawnPrefabs, enemyZoneChance, chunkCoord, new Vector2Int(x, y), chunkParent, newData);
            }
        }

        // --- PASS 2: HOUSES (Limit: 2) ---
        int housesSpawned = 0;
        const int maxHousesPerChunk = 1;

        // Use a bool to break out of the nested loop entirely
        bool limitReached = false;

        for (int x = 0; x < chunkSize && !limitReached; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;
                float height = terrainGenerator.GetHeight(worldX, worldY, worldSize);

                if (height < terrainGenerator.sandyGrassLevel) continue;

                if (Random.value < houseChance)
                {
                    GameObject structurePrefab = GetRandomStructurePrefab();
                    if (structurePrefab == null) continue;

                    Vector3 pos = new Vector3(worldX + 0.5f, worldY + 0.5f, 0f);
                    GameObject go = InstantiateAndRecord(structurePrefab, pos, chunkParent, null, newData);

                    StructureVisibility sv = go.GetComponent<StructureVisibility>();
                    if (sv != null) sv.ClearObstacles();

                    housesSpawned++;
                    limitReached = true;
                    break;
                }
            }
        }

        //Debug.Log($"[SPAWNER] Chunk {chunkCoord} complete. House Attempts: {houseAttempts} | Spawned: {housesSpawned}");
        WorldSaveData.Instance.SaveChunkData(chunkCoord, newData);
    }

    private GameObject GetRandomStructurePrefab()
    {
        if (structures == null || structures.Length == 0) return null;

        float totalWeight = 0;
        foreach (var s in structures) totalWeight += s.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0;

        foreach (var s in structures)
        {
            cumulative += s.weight;
            if (roll <= cumulative) return s.prefab;
        }
        return structures[0].prefab;
    }

    #region Spawning Helpers

    void SpawnFromChunkData(ChunkData data, Transform parent)
    {
        foreach (var objData in data.objects)
        {
            if (!prefabLookup.TryGetValue(objData.prefabName, out GameObject prefab))
                continue;

            GameObject go = Instantiate(prefab, objData.position, Quaternion.identity, parent);

            // Get the containers in the same order they were recorded
            WorldContainer[] containers = go.GetComponentsInChildren<WorldContainer>();

            for (int i = 0; i < containers.Length; i++)
            {
                // Safety check: make sure we have a saved ID for this container index
                if (i < objData.containerIds.Count)
                {
                    Vector2Int cell = new Vector2Int(Mathf.FloorToInt(containers[i].transform.position.x), Mathf.FloorToInt(containers[i].transform.position.y));

                    // Pass the SAVED ID back to the container
                    containers[i].Initialize(cell, objData.containerIds[i]);
                }

                StructureVisualVariator variator = go.GetComponent<StructureVisualVariator>();
                if (variator != null && objData.hiddenObjectIndices != null)
                {
                    variator.ApplyVariation(objData.hiddenObjectIndices);
                }
            }
        }

        // 2. NEW: Restore Saved Enemies
        Debug.Log($"[OBJECT SPAWNER] Restoring {data.enemies.Count} enemies in chunk {data.chunkCoord}");
        foreach (var enemyData in data.enemies)
        {
            // We call the manager we built earlier to handle the spawning
            EnemySpawnManager.Instance.RestoreEnemy(enemyData);
        }
    }

    void TrySpawn(GameObject prefab, float chance, Vector3 pos, Transform parent, ChunkData chunkData)
    {
        if (prefab != null && Random.value < chance)
        {
            InstantiateAndRecord(prefab, pos, parent, null, chunkData);
        }
    }

    void TrySpawnRandom(GameObject[] prefabs, float chance, Vector2Int chunkCoord, Vector2Int localTileCoord, Transform parent, ChunkData chunkData)
    {
        if (prefabs.Length == 0 || Random.value >= chance)
            return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];

        int worldX = chunkCoord.x * 64 + localTileCoord.x;
        int worldY = chunkCoord.y * 64 + localTileCoord.y;
        Vector3 pos = new Vector3(worldX + 0.5f, worldY + 0.5f, 0f);

        // REMOVED: Don't generate the ID here! 
        // Just pass null; the container will generate its own inside Initialize.
        InstantiateAndRecord(prefab, pos, parent, null, chunkData);
    }

    GameObject InstantiateAndRecord(GameObject prefab, Vector3 pos, Transform parent, string singleId, ChunkData chunkData)
    {
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);

        // 1. Find all containers inside (works for 1 chest or 10 cupboards in a house)
        WorldContainer[] containers = go.GetComponentsInChildren<WorldContainer>();
        List<string> savedIds = new List<string>();

        foreach (var container in containers)
        {
            Vector2Int cell = new Vector2Int(Mathf.FloorToInt(container.transform.position.x), Mathf.FloorToInt(container.transform.position.y));

            // Initialize it (this generates the uniqueId if it doesn't have one)
            container.Initialize(cell, null);

            // Record the ID that was just generated
            savedIds.Add(container.uniqueId);
        }

        StructureVisualVariator variator = go.GetComponent<StructureVisualVariator>();
        List<int> hiddenIndices = new List<int>();
        if (variator != null)
        {
            hiddenIndices = variator.GenerateVariation();
        }

        // 2. Save the object data including the list of IDs
        chunkData.objects.Add(new SpawnedObjectData
        {
            prefabName = prefab.name,
            position = pos,
            containerIds = savedIds,
            hiddenObjectIndices = hiddenIndices
        });

        return go;
    }

    #endregion
}
