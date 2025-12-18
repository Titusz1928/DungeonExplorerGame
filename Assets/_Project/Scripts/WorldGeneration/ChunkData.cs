using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;
    public List<SpawnedObjectData> objects = new();
}

[System.Serializable]
public class SpawnedObjectData
{
    public string prefabName;
    public Vector3 position;
    public string containerId; // null if object has no container
}
