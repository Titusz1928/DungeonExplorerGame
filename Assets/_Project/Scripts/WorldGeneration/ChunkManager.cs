using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
    [Header("World Settings")]
    public int chunksPerAxis = 20;
    public int chunkSize = 64;
    private static int _chunkSize = 64;

    [Header("References")]
    public Transform player;
    public Grid grid;
    public WorldTileSet tileSet;

    private Dictionary<Vector2Int, Tilemap> loadedChunks = new();
    private Vector2Int lastPlayerChunk;

    public TileTerrainGenerator terrainGenerator;

    public WorldObjectSpawner worldObjectSpawner;

    private System.Collections.IEnumerator Start()
    {
        FindPersistentReferences();

        int worldSize = chunksPerAxis * chunkSize;

        //Vector2 spawnPos = terrainGenerator.FindRandomCoastlineSpawn(worldSize, GameSettings.Instance.seed);

        //if (player != null)
        //{
        //    player.position = spawnPos;
        //}


        // Wait until WorldSaveData has finished loading the file
        while (WorldSaveData.Instance != null && !WorldSaveData.Instance.IsLoaded)
        {
            yield return null;
        }

        lastPlayerChunk = GetPlayerChunk();
        UpdateChunks(true);
    }

    public static int getChunkSize()
    {
        return _chunkSize;
    }

    private void FindPersistentReferences()
    {
        // Find the Player (using the static reference we built earlier)
        if (player == null)
        {
            player = PlayerReference.PlayerTransform;

            // Fallback if PlayerReference isn't ready
            if (player == null)
            {
                GameObject pObj = GameObject.Find("PLAYER");
                if (pObj != null) player = pObj.transform;
            }
        }

        // Find the Terrain Generator
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<TileTerrainGenerator>();
        }

        // Find the Spawner (if it's attached to the Generator)
        if (worldObjectSpawner == null && terrainGenerator != null)
        {
            worldObjectSpawner = terrainGenerator.GetComponent<WorldObjectSpawner>();
        }
    }

    void Update()
    {
        Vector2Int currentChunk = GetPlayerChunk();

        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            UpdateChunks();
        }
    }

    Vector2Int GetPlayerChunk()
    {
        int cx = Mathf.FloorToInt(player.position.x / chunkSize);
        int cy = Mathf.FloorToInt(player.position.y / chunkSize);
        return new Vector2Int(cx, cy);
    }

    void UpdateChunks(bool force = false)
    {
        HashSet<Vector2Int> neededChunks = new();

        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector2Int coord = lastPlayerChunk + new Vector2Int(x, y);

                if (IsChunkInWorld(coord))
                {
                    neededChunks.Add(coord);

                    if (!loadedChunks.ContainsKey(coord))
                        LoadChunk(coord);
                }
            }
        }

        List<Vector2Int> toUnload = new();

        foreach (var coord in loadedChunks.Keys)
        {
            if (!neededChunks.Contains(coord))
                toUnload.Add(coord);
        }

        foreach (var coord in toUnload)
            UnloadChunk(coord);
    }

    bool IsChunkInWorld(Vector2Int coord)
    {
        return coord.x >= 0 && coord.y >= 0 &&
               coord.x < chunksPerAxis &&
               coord.y < chunksPerAxis;
    }

    void LoadChunk(Vector2Int coord)
    {
        GameObject chunkGO = new($"Chunk_{coord.x}_{coord.y}");
        chunkGO.transform.parent = grid.transform;
        chunkGO.transform.position = new Vector3(
            coord.x * chunkSize,
            coord.y * chunkSize,
            0
        );

        Tilemap tilemap = chunkGO.AddComponent<Tilemap>();
        chunkGO.AddComponent<TilemapRenderer>();

        StartCoroutine(
            terrainGenerator.GenerateChunkAsync(tilemap, coord, chunkSize)
        );

        loadedChunks.Add(coord, tilemap);

        worldObjectSpawner.SpawnObjectsInChunk(coord, chunkSize, chunkGO.transform);
    }

    void UnloadChunk(Vector2Int coord)
    {
        if (loadedChunks.TryGetValue(coord, out Tilemap tilemap))
        {
            Destroy(tilemap.gameObject);
            loadedChunks.Remove(coord);
        }
    }

}
