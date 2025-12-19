using System.IO;
using UnityEngine;

public static class SaveSystem
{

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void SaveGame()
    {
        Debug.Log("attempting to save game");

        // Break these out to see exactly which one crashes
        var worldData = WorldSaveData.Instance.BuildWorldSave();
        var playerData = PlayerSaveBuilder.Build();
        var settingsInstance = GameSettings.Instance; // Check if Instance is the problem

        SaveGame save = new SaveGame();
        save.world = worldData;
        save.player = playerData;
        save.playTime = Time.time;

        save.gameSettings = new GameSettingsSave();
        save.gameSettings.seed = settingsInstance.seed;
        save.gameSettings.difficulty = settingsInstance.difficulty;

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game saved");
    }

    public static SaveGame LoadGame()
    {
        if (!SaveFileExists()) return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveGame>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load save: {e.Message}");
            return null;
        }
    }

    public static bool SaveFileExists() => File.Exists(SavePath);
}
