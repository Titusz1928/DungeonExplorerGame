[System.Serializable]
public class WorldMeta
{
    public string worldId;
    public string worldName;
    public string createdAt;
    public string updatedAt;
    public int seed;
    public float difficulty;
}

[System.Serializable]
public class SaveGame
{
    public WorldSave world;
    public PlayerSave player;
    public float playTime;
    public WorldMeta worldMetaData; 
}

