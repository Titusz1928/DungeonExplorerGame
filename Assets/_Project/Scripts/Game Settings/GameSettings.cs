using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("World Identity")]
    public string worldId;        // unique, never changes
    public string worldName;      // player-facing
    public string createdAt;      // ISO string for JSON

    [Header("World Data")]
    public int seed;
    public bool loadFromSave;

    [Header("Difficulty")]
    public float difficulty = 1f; //placeholder

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CreateNewWorld(string name, int seedValue)
    {
        worldId = Guid.NewGuid().ToString();
        worldName = string.IsNullOrWhiteSpace(name) ? "New World" : name;
        createdAt = DateTime.UtcNow.ToString("o"); // ISO 8601

        seed = seedValue;
        loadFromSave = false;
    }


    /// <summary>
    /// Generate a new random seed for new games
    /// </summary>
    public void GenerateNewSeed()
    {
        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
    }
}
