using UnityEngine;
using System.IO;

public static class SaveLoadManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void Save(SaveGame data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game Saved to {SavePath}");
    }

    public static SaveGame Load()
    {
        if (!File.Exists(SavePath)) return null;

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveGame>(json);
    }

    public static bool SaveExists() => File.Exists(SavePath);
}