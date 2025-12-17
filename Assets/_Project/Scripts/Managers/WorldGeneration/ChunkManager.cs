using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : MonoBehaviour
{
    [Header("World Settings")]
    public int chunksPerAxis = 20;
    public int chunkSize = 64;

    [Header("References")]
    public Transform player;
    public Grid grid;
    public WorldTileSet tileSet;

    private Dictionary<Vector2Int, Tilemap> loadedChunks = new();
    private Vector2Int lastPlayerChunk;

    public TileTerrainGenerator terrainGenerator;

    void Start()
    {
        int worldSize = chunksPerAxis * chunkSize;

        Vector2 spawnPos = terrainGenerator.FindRandomCoastlineSpawn(worldSize);

        player.position = spawnPos;

        lastPlayerChunk = GetPlayerChunk();
        UpdateChunks(true);
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
