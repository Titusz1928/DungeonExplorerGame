[System.Serializable]
public class GameSettingsSave
{
    public int seed;
    public float difficulty;
}

[System.Serializable]
public class SaveGame
{
    public WorldSave world;
    public PlayerSave player;
    public float playTime;
    public GameSettingsSave gameSettings;
}

