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
        //Debug.Log("[SAVE SYSTEM] Starting BuildWorldSave...");
        WorldSave save = new WorldSave();

        // 1. Prepare Chunks
        foreach (var kvp in chunkData)
        {
            // We clear the enemy list before repopulating it with currently active enemies
            kvp.Value.enemies.Clear();
        }

        // 2. Find and Categorize Enemies
        EnemyController[] activeEnemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        //Debug.Log($"[SAVE SYSTEM] Found {activeEnemies.Length} active EnemyControllers in scene.");

        foreach (var enemy in activeEnemies)
        {
            EnemyStats stats = enemy.GetComponent<EnemyStats>();
            if (stats == null)
            {
                //Debug.LogWarning($"[SAVE SYSTEM] Enemy {enemy.name} at {enemy.transform.position} missing EnemyStats! Skipping.");
                continue;
            }

            if (stats.currentHP <= 0)
            {
                //Debug.Log($"[SAVE SYSTEM] Enemy {enemy.name} is dead (HP: {stats.currentHP}). Not saving.");
                continue;
            }

            Vector2Int coord = GetChunkCoordFromPosition(enemy.transform.position);
            string coordKey = $"{coord.x}_{coord.y}";

            if (!chunkData.TryGetValue(coordKey, out ChunkData chunk))
            {
                //Debug.Log($"[SAVE SYSTEM] Enemy {enemy.name} is in a new/unsaved chunk {coordKey}. Creating ChunkData.");
                chunk = new ChunkData { chunkCoord = coord };
                chunkData.Add(coordKey, chunk);
            }

            EnemySaveData data = new EnemySaveData
            {
                instanceID = enemy.instanceID,
                enemyID = enemy.data.enemyID,
                position = enemy.transform.position,
                currentHP = stats.currentHP,
                currentState = enemy.GetState(),
                guardCenter = enemy.GetGuardCenter()
            };

            chunk.enemies.Add(data);
            //Debug.Log($"[SAVE SYSTEM] Saved Enemy: {enemy.name} (ID: {enemy.data.enemyID}) to Chunk: {coordKey} at Pos: {data.position}");
        }

        // 3. FINAL COMPILATION
        // IMPORTANT: We iterate through chunkData AFTER the enemy loop to ensure 
        // enemies added to new chunks are included in the final save file.
        foreach (var kvp in chunkData)
        {
            save.chunks.Add(kvp.Value);
            if (kvp.Value.enemies.Count > 0)
            {
                //Debug.Log($"[SAVE SYSTEM] Chunk {kvp.Key} finalized with {kvp.Value.enemies.Count} enemies.");
            }
        }

        // 4. Save Containers
        foreach (var kvp in containerData)
        {
            save.containers.Add(kvp.Value);
        }

        //Debug.Log($"[SAVE SYSTEM] BuildWorldSave Complete. Total Chunks: {save.chunks.Count}, Total Containers: {save.containers.Count}");
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

        // 1. Restore Container Data FIRST
        // We do this first so that when objects look up IDs, the data is already there
        foreach (var savedContainer in worldSave.containers)
        {
            if (savedContainer != null)
            {
                containerData[savedContainer.id] = savedContainer;
            }
        }

        // 2. Restore Chunks and Re-map IDs
        foreach (var chunk in worldSave.chunks)
        {
            string key = GetChunkKey(chunk.chunkCoord);
            chunkData[key] = chunk;

            foreach (var obj in chunk.objects)
            {
                // If the list is null or empty, skip
                if (obj.containerIds == null || obj.containerIds.Count == 0) continue;

                // Handle mapping
                if (obj.containerIds.Count == 1)
                {
                    // Simple case: Single chest/object
                    Vector2Int cell = new Vector2Int(Mathf.FloorToInt(obj.position.x), Mathf.FloorToInt(obj.position.y));
                    worldCellToId[cell] = obj.containerIds[0];
                }
                else
                {

                    foreach (string id in obj.containerIds)
                    {
                        RegisterIdToCellFromIdString(id);
                    }
                }
            }
        }

        IsLoaded = true;
        Debug.Log($"Restored {chunkData.Count} chunks and {containerData.Count} containers. System is READY.");
    }

    private void RegisterIdToCellFromIdString(string id)
    {
        string[] parts = id.Split('_');
        if (parts.Length >= 3)
        {
            if (int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
            {
                worldCellToId[new Vector2Int(x, y)] = id;
            }
        }
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
        Debug.Log("[CONTAINER DATA] saving for: " + id);
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

    public void RemoveObjectFromChunk(Vector3 position)
    {
        Vector2Int chunkCoord = GetChunkCoordFromPosition(position);
        string key = GetChunkKey(chunkCoord);

        if (chunkData.TryGetValue(key, out ChunkData data))
        {
            // Remove any object that is within 0.1 units of the target position
            int removedCount = data.objects.RemoveAll(obj =>
                Vector2.Distance(obj.position, position) < 0.1f);

            if (removedCount > 0)
            {
                Debug.Log($"[SAVE SYSTEM] Removed {removedCount} object(s) at {position} from Chunk {key}");
            }
        }
    }
}
