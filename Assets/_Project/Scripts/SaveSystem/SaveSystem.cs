using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveSystem
{

    private static string SavesRoot =>
    Path.Combine(Application.persistentDataPath, "Saves");

    // Helper to get a specific world folder without needing GameSettings.Instance
    private static string GetWorldPath(string worldId) => Path.Combine(SavesRoot, worldId);

    private static string WorldFolder =>
        Path.Combine(SavesRoot, GameSettings.Instance.worldId);

    private static string MetaPath =>
    Path.Combine(WorldFolder, "meta.json");

    private static string WorldSavePath =>
        Path.Combine(WorldFolder, "world.json");


    // --- NEW: Delete World ---
    public static void DeleteWorld(string worldId)
    {
        string path = GetWorldPath(worldId);
        if (Directory.Exists(path))
        {
            // 'true' allows deleting the directory and all its contents (meta.json, world.json)
            Directory.Delete(path, true);
            Debug.Log($"Successfully deleted world folder: {worldId}");
        }
        else
        {
            Debug.LogWarning($"Attempted to delete non-existent world: {worldId}");
        }
    }

    // --- NEW: Update World Meta ---
    public static void UpdateWorldMeta(WorldMeta updatedMeta)
    {
        string path = GetWorldPath(updatedMeta.worldId);
        string metaFile = Path.Combine(path, "meta.json");

        if (!Directory.Exists(path))
        {
            Debug.LogError($"Cannot update meta: Folder for {updatedMeta.worldId} does not exist.");
            return;
        }

        // We update the timestamp since the file is being modified
        updatedMeta.updatedAt = System.DateTime.UtcNow.ToString("o");

        try
        {
            string json = JsonUtility.ToJson(updatedMeta, true);
            File.WriteAllText(metaFile, json);
            Debug.Log($"Updated metadata for: {updatedMeta.worldName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update meta: {e.Message}");
        }
    }



    public static void SaveGame()
    {
        Debug.Log("Attempting to save game");

        if (!Directory.Exists(WorldFolder))
            Directory.CreateDirectory(WorldFolder);

        SaveMeta();
        SaveWorld();

        Debug.Log($"Game saved to {WorldFolder}");
    }


    private static void SaveWorld()
    {
        SaveGame save = new SaveGame
        {
            world = WorldSaveData.Instance.BuildWorldSave(),
            player = PlayerSaveBuilder.Build(),
            playTime = Time.time
        };

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(WorldSavePath, json);
    }

    private static void SaveMeta()
    {
        var settings = GameSettings.Instance;

        WorldMeta meta = new WorldMeta
        {
            worldId = settings.worldId,
            worldName = settings.worldName,
            createdAt = settings.createdAt,
            updatedAt = System.DateTime.UtcNow.ToString("o"),
            seed = settings.seed,
            difficulty = settings.difficulty
        };

        string json = JsonUtility.ToJson(meta, true);
        File.WriteAllText(MetaPath, json);
    }

    public static SaveGame LoadGame(string worldId)
    {
        string folder = Path.Combine(SavesRoot, worldId);
        string worldPath = Path.Combine(folder, "world.json");
        string metaPath = Path.Combine(folder, "meta.json");

        if (!File.Exists(worldPath) || !File.Exists(metaPath))
            return null;

        try
        {
            // Load world save
            string worldJson = File.ReadAllText(worldPath);
            SaveGame save = JsonUtility.FromJson<SaveGame>(worldJson);

            // Load metadata
            string metaJson = File.ReadAllText(metaPath);
            WorldMeta meta = JsonUtility.FromJson<WorldMeta>(metaJson);

            // Attach metadata
            save.worldMetaData = meta;

            return save;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load save for world {worldId}: {e.Message}");
            return null;
        }
    }

    public static List<WorldMeta> LoadAllWorldMeta()
    {
        List<WorldMeta> worlds = new();

        if (!Directory.Exists(SavesRoot))
            return worlds;

        foreach (var dir in Directory.GetDirectories(SavesRoot))
        {
            string metaPath = Path.Combine(dir, "meta.json");
            if (!File.Exists(metaPath))
                continue;

            try
            {
                string json = File.ReadAllText(metaPath);
                worlds.Add(JsonUtility.FromJson<WorldMeta>(json));
            }
            catch { /* skip corrupted entries */ }
        }

        return worlds;
    }



}
