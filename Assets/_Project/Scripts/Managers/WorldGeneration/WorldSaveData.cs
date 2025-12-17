using System.Collections.Generic;
using UnityEngine;

public class WorldSaveData : MonoBehaviour
{
    public static WorldSaveData Instance;

    // Containers: key = containerId, value = items
    private Dictionary<string, List<(ItemSO item, int qty)>> containerData = new();

    // Mapping from world coordinates to containerId
    private Dictionary<Vector2Int, string> worldCellToId = new();

    // Chunk data: key = chunk coordinates, value = saved objects in chunk
    private Dictionary<Vector2Int, ChunkData> chunkData = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Container ID

    public string GetOrCreateContainerId(Vector2Int cell, string type)
    {
        if (worldCellToId.TryGetValue(cell, out string existingId))
            return existingId;

        string newId = $"{type}_{cell.x}_{cell.y}_{System.Guid.NewGuid()}";
        worldCellToId[cell] = newId;
        return newId;
    }

    #endregion

    #region Container Data

    public bool HasContainerData(string id) => containerData.ContainsKey(id);

    public List<(ItemSO item, int qty)> GetContainerData(string id) => containerData[id];

    public void SaveContainerData(string id, List<(ItemSO item, int qty)> items)
    {
        containerData[id] = new List<(ItemSO, int)>(items);
    }

    #endregion

    #region Chunk Data

    public bool HasChunkData(Vector2Int chunkCoord) => chunkData.ContainsKey(chunkCoord);

    public ChunkData GetChunkData(Vector2Int chunkCoord) => chunkData[chunkCoord];

    public void SaveChunkData(Vector2Int chunkCoord, ChunkData data)
    {
        chunkData[chunkCoord] = data;
    }

    #endregion
}
