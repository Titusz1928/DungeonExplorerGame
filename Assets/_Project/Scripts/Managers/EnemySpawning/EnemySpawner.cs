using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int enemiesToSpawn = 3;
    public float spawnRadius = 6f;

    [Header("Enemy Types")]
    public List<int> allowedEnemyIndices;

    private bool hasSpawned = false;

    void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            int enemyIndex = allowedEnemyIndices[
                Random.Range(0, allowedEnemyIndices.Count)
            ];

            EnemySpawnManager.Instance.SpawnEnemy(
                enemyIndex,
                spawnPos,
                transform   // parent to area
            );
        }
    }
}
