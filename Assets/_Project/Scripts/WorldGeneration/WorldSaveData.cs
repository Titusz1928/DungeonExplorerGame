using System.Collections.Generic;
using UnityEngine;

public class WorldSaveData : MonoBehaviour
{
    public static WorldSaveData Instance;

    // Containers: key = containerId, value = items
    private Dictionary<string, ContainerSaveData> containerData = new();

    // Mapping from world coordinates to containerId
    private Dictionary<Vector2Int, string> worldCellToId = new();

    // Chunk data: key = chunk coordinates, value = saved objects in chunk
    private Dictionary<string, ChunkData> chunkData = new();

    public bool IsLoaded { get; private set; } = false;

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

    // Call this if no save file exists to let the game start
    public void InitializeNewGame()
    {
        chunkData.Clear();
        containerData.Clear();
        worldCellToId.Clear();
        IsLoaded = true;
        Debug.Log("[SAVE SYSTEM] Initialized New Game (No save found).");
    }

    public WorldSave BuildWorldSave()
    {
        WorldSave save = new WorldSave();

        // ------------------------
        // SAVE CHUNKS
        // ------------------------
        foreach (var kvp in chunkData)
        {
            // ChunkData is already pure data
            save.chunks.Add(kvp.Value);
            kvp.Value.enemies.Clear();
        }

        // 2. Find Enemies
        EnemyController[] activeEnemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (var enemy in activeEnemies)
        {
            // Access the Stats component instead of the Controller for HP
            EnemyStats stats = enemy.GetComponent<EnemyStats>();

            // Safety check: if for some reason the enemy has no stats, we skip or assume it's alive
            if (stats == null) continue;

            // ONLY save the enemy if it's actually alive!
            if (stats.currentHP <= 0) continue;

            Vector2Int coord = GetChunkCoordFromPosition(enemy.transform.position);
            string coordKey = $"{coord.x}_{coord.y}";

            if (!chunkData.TryGetValue(coordKey, out ChunkData chunk))
            {
                chunk = new ChunkData { chunkCoord = coord };
                chunkData.Add(coordKey, chunk);
            }

            // Add the enemy to the chunk
            chunk.enemies.Add(new EnemySaveData
            {
                instanceID = enemy.instanceID,
                enemyID = enemy.data.enemyID,
                position = enemy.transform.position,
                currentHP = stats.currentHP, // Updated to use stats
                currentState = enemy.GetState(),
                guardCenter = enemy.GetGuardCenter()
            });
        }

        // ------------------------
        // SAVE CONTAINERS
        // ------------------------
        // FIXED: Convert Dictionary values into the List
        foreach (var kvp in containerData)
        {
            save.containers.Add(kvp.Value);
        }


        return save;
    }

    public Vector2Int GetChunkCoordFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / ChunkManager.getChunkSize());
        int y = Mathf.FloorToInt(position.y / ChunkManager.getChunkSize());
        return new Vector2Int(x, y);
    }


    public void LoadFromWorldSave(WorldSave worldSave)
    {
        chunkData.Clear();
        containerData.Clear();
        worldCellToId.Clear();

        // 1. Restore Chunks
        foreach (var chunk in worldSave.chunks)
        {
            // Use the string key helper
            string key = GetChunkKey(chunk.chunkCoord);
            chunkData[key] = chunk;

            // 2. Re-map Container IDs
            foreach (var obj in chunk.objects)
            {
                if (!string.IsNullOrEmpty(obj.containerId))
                {
                    // Use FloorToInt to ensure (417.5, 500.5) becomes (417, 500)
                    Vector2Int cell = new Vector2Int(Mathf.FloorToInt(obj.position.x), Mathf.FloorToInt(obj.position.y));
                    worldCellToId[cell] = obj.containerId;
                }
            }
        }

        // 3. Restore Container Data
        foreach (var savedContainer in worldSave.containers)
        {
            if (savedContainer != null)
            {
                containerData[savedContainer.id] = savedContainer;
            }
        }

        Debug.Log($"Restored {chunkData.Count} chunks.");

        IsLoaded = true; // Mark as ready
        Debug.Log($"Restored {chunkData.Count} chunks. System is now READY.");
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

    // CHANGED: This now "unwraps" the SaveData back into a runtime List
    public List<ItemInstance> GetContainerData(string id)
    {
        // If no data exists, return an empty list of instances
        if (!containerData.TryGetValue(id, out ContainerSaveData savedData))
            return new List<ItemInstance>();

        List<ItemInstance> runtimeList = new List<ItemInstance>();

        foreach (var entry in savedData.items.entries)
        {
            ItemSO itemSO = ItemDatabase.instance.GetByID(entry.itemID);
            if (itemSO != null)
            {
                // Create the instance
                ItemInstance inst = new ItemInstance(itemSO, entry.quantity);

                // RESTORE the saved state
                inst.currentDurability = entry.durability;
                // If your ItemSaveEntry has a 'holes' field, add it here too:
                // inst.holes = entry.holes; 

                runtimeList.Add(inst);
            }
        }

        return runtimeList;
    }

    // NEW: Helper to check the initialized state from the stored object
    public bool IsContainerInitialized(string id)
    {
        return containerData.ContainsKey(id) && containerData[id].initialized;
    }

    // CHANGED: This now "wraps" the runtime List into a ContainerSaveData object
    public void SaveContainerData(string id, List<ItemInstance> items, bool wasopened, bool isInitialized = true)
    {
        ContainerSaveData newData = new ContainerSaveData
        {
            id = id,
            initialized = isInitialized,
            wasOpened = wasopened,
            items = new InventorySaveData()
        };

        foreach (var inst in items)
        {
            newData.items.entries.Add(new ItemSaveEntry
            {
                itemID = inst.itemSO.ID,
                quantity = inst.quantity,
                durability = inst.currentDurability // NOW SAVING DURABILITY
                                                    // holes = inst.holes // Uncomment if you added this to ItemSaveEntry
            });
        }

        containerData[id] = newData;
    }

    #endregion

    #region Chunk Data

    private string GetChunkKey(Vector2Int coord) => $"{coord.x}_{coord.y}";

    public bool HasChunkData(Vector2Int chunkCoord)
    {
        string key = GetChunkKey(chunkCoord);
        bool exists = chunkData.ContainsKey(key);

        if (!exists && chunkData.Count > 0)
        {
            Debug.Log($"[CHUNK MISS] Looking for {key}. Dictionary has {chunkData.Count} entries.");
        }
        return exists;
    }

    public ChunkData GetChunkData(Vector2Int chunkCoord)
    {
        return chunkData[GetChunkKey(chunkCoord)];
    }

    public void SaveChunkData(Vector2Int chunkCoord, ChunkData data)
    {
        // Ensure the data itself knows its coord before saving
        data.chunkCoord = chunkCoord;
        chunkData[GetChunkKey(chunkCoord)] = data;
    }

    #endregion


    public void UpdateObjectPrefabInChunk(Vector3 position, string newPrefabName)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(position);
        string key = GetChunkKey(chunkCoord);

        if (chunkData.TryGetValue(key, out ChunkData data))
        {
            // Find the object in this chunk that matches the position
            // We use a small epsilon (0.1f) in case of float precision errors
            SpawnedObjectData objectToUpdate = data.objects.Find(obj =>
                Vector2.Distance(obj.position, position) < 0.1f);

            if (objectToUpdate != null)
            {
                objectToUpdate.prefabName = newPrefabName;
                Debug.Log($"[SAVE SYSTEM] Updated {position} to {newPrefabName} in Chunk {key}");
            }
        }
    }
}
