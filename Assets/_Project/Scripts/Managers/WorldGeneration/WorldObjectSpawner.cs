using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldObjectSpawner : MonoBehaviour
{
    [Header("References")]
    public TileTerrainGenerator terrainGenerator;
    public Tilemap tilemap; // optional

    [Header("Prefabs")]
    public GameObject treePrefab;
    public GameObject[] bushPrefabs;
    public GameObject[] chestPrefabs;
    public GameObject[] enemySpawnPrefabs;

    [Header("Spawn Settings")]
    public float treeChance = 0.02f;   // 2% per grass tile
    public float bushChance = 0.03f;   // 3% per grass tile
    public float chestChance = 0.005f; // 0.5% per grass tile
    public float enemyZoneChance = 0.005f;

    public int worldSize = 1280;


    public void SpawnObjectsInChunk(Vector2Int chunkCoord, int chunkSize)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;

                float height = terrainGenerator.GetHeight(worldX, worldY, worldSize);
                if (height >= terrainGenerator.sandyGrassLevel)
                {
                    Vector3 pos = new Vector3(worldX + 0.5f, worldY + 0.5f, 0f);

                    if (Random.value < treeChance && treePrefab != null)
                        Instantiate(treePrefab, pos, Quaternion.identity, transform);

                    if (Random.value < bushChance && bushPrefabs.Length > 0)
                    {
                        GameObject bush = bushPrefabs[Random.Range(0, bushPrefabs.Length)];
                        Instantiate(bush, pos, Quaternion.identity, transform);
                    }

                    if (Random.value < chestChance && chestPrefabs.Length > 0)
                    {
                        GameObject chest = chestPrefabs[Random.Range(0, chestPrefabs.Length)];
                        Instantiate(chest, pos, Quaternion.identity, transform);
                    }

                    if (Random.value < enemyZoneChance && enemySpawnPrefabs.Length > 0)
                    {
                        GameObject enemyZone = enemySpawnPrefabs[Random.Range(0, enemySpawnPrefabs.Length)];
                        Instantiate(enemyZone, pos, Quaternion.identity, transform);
                    }
                }
            }
        }
    }

}
