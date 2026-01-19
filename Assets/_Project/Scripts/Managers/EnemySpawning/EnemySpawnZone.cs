using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public float radius = 8f;
    public int maxEnemiesInZone = 3;

    [Header("Enemy Types")]
    public List<int> allowedEnemyIndices;

    private readonly List<GameObject> aliveEnemies = new();

    protected virtual void OnEnable()
    {
        if (EnemySpawnManager.Instance != null)
            EnemySpawnManager.Instance.RegisterZone(this);
    }

    protected virtual void OnDisable()
    {
        if (EnemySpawnManager.Instance != null)
            EnemySpawnManager.Instance.UnregisterZone(this);
    }


    public virtual bool CanSpawnInZone()
    {
        aliveEnemies.RemoveAll(e => e == null);
        return aliveEnemies.Count < maxEnemiesInZone;
    }

    public Vector3 GetRandomPoint()
    {
        return transform.position + (Vector3)(Random.insideUnitCircle * radius);
    }

    public int GetRandomEnemyIndex()
    {
        return allowedEnemyIndices[Random.Range(0, allowedEnemyIndices.Count)];
    }

    public void RegisterEnemy(GameObject enemy)
    {
        aliveEnemies.Add(enemy);
    }
}
