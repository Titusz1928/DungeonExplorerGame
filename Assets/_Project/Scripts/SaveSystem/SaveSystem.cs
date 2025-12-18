using System.IO;
using UnityEngine;

public static class SaveSystem
{

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "savegame.json");

    public static void SaveGame()
    {
        Debug.Log("attempting to save game");

        SaveGame save = new SaveGame
        {
            world = WorldSaveData.Instance.BuildWorldSave(),
            player = PlayerSaveBuilder.Build(),
            playTime = Time.time,
            gameSettings = new GameSettingsSave
            {
                seed = GameSettings.Instance.seed,
                difficulty = GameSettings.Instance.difficulty
            }
        };

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game saved");
    }
}
