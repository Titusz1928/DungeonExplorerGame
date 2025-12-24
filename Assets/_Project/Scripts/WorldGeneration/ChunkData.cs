using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;
    public List<SpawnedObjectData> objects = new();
    public List<EnemySaveData> enemies = new();
}

[System.Serializable]
public class SpawnedObjectData
{
    public string prefabName;
    public Vector2 position;
    public string containerId; // null if object has no container
}
