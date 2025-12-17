using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldObjectSpawner : MonoBehaviour
{
    [Header("References")]
    public TileTerrainGenerator terrainGenerator;

    [Header("Prefabs")]
    public GameObject treePrefab;
    public GameObject[] bushPrefabs;
    public GameObject[] chestPrefabs;
    public GameObject[] enemySpawnPrefabs;

    [Header("Spawn Settings")]
    public float treeChance = 0.02f;
    public float bushChance = 0.03f;
    public float chestChance = 0.005f;
    public float enemyZoneChance = 0.005f;

    public int worldSize = 1280;

    // Lookup for prefab by name
    private Dictionary<string, GameObject> prefabLookup;

    void Awake()
    {
        prefabLookup = new Dictionary<string, GameObject>();
        if (treePrefab != null) prefabLookup[treePrefab.name] = treePrefab;
        foreach (var p in bushPrefabs) prefabLookup[p.name] = p;
        foreach (var p in chestPrefabs) prefabLookup[p.name] = p;
        foreach (var p in enemySpawnPrefabs) prefabLookup[p.name] = p;
    }

    public void SpawnObjectsInChunk(Vector2Int chunkCoord, int chunkSize, Transform chunkParent)
    {
        // Check if we already have saved chunk data
        if (WorldSaveData.Instance.HasChunkData(chunkCoord))
        {
            ChunkData data = WorldSaveData.Instance.GetChunkData(chunkCoord);
            SpawnFromChunkData(data, chunkParent);
            return;
        }

        // Otherwise, generate chunk for the first time
        ChunkData newData = new ChunkData() { chunkCoord = chunkCoord };

        Random.InitState(chunkCoord.x * 73856093 ^ chunkCoord.y * 19349663 ^ terrainGenerator.seed);

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

        WorldSaveData.Instance.SaveChunkData(chunkCoord, newData);
    }

    #region Spawning Helpers

    void SpawnFromChunkData(ChunkData data, Transform parent)
    {
        foreach (var objData in data.objects)
        {
            if (!prefabLookup.TryGetValue(objData.prefabName, out GameObject prefab))
                continue;

            GameObject go = Instantiate(prefab, objData.position, Quaternion.identity, parent);
            if (objData.containerId != null)
            {
                WorldContainer container = go.GetComponentInChildren<WorldContainer>();
                if (container != null)
                    container.Initialize(new Vector2Int((int)objData.position.x, (int)objData.position.y), objData.containerId);
            }
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

        string containerId = null;
        WorldContainer container = prefab.GetComponentInChildren<WorldContainer>();
        if (container != null)
        {
            containerId = WorldSaveData.Instance.GetOrCreateContainerId(new Vector2Int(worldX, worldY), prefab.name);
        }

        InstantiateAndRecord(prefab, pos, parent, containerId, chunkData);
    }

    void InstantiateAndRecord(GameObject prefab, Vector3 pos, Transform parent, string containerId, ChunkData chunkData)
    {
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, parent);

        if (containerId != null)
        {
            WorldContainer container = go.GetComponentInChildren<WorldContainer>();
            if (container != null)
                container.Initialize(new Vector2Int((int)pos.x, (int)pos.y), containerId);
        }

        chunkData.objects.Add(new SpawnedObjectData
        {
            prefabName = prefab.name,
            position = pos,
            containerId = containerId
        });
    }

    #endregion
}
