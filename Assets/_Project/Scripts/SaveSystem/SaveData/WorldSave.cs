using System.Collections.Generic;

[System.Serializable]
public class WorldSave
{
    public List<ChunkData> chunks = new();
    public List<EnemySaveData> enemies = new();
    public Dictionary<string, ContainerSaveData> containers = new();
}
