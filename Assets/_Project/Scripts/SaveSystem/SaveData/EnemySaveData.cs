using UnityEngine;

[System.Serializable]
public class EnemySaveData
{
    public string prefabName;
    public string instanceID;
    public int enemyID;
    public Vector2 position;
    public int currentHP;
    public EnemyState currentState;
    public Vector2 guardCenter;
}