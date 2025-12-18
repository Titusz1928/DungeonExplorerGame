using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Game Settings")]
    public int seed;
    public float difficulty = 1f; // placeholder

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

    /// <summary>
    /// Generate a new random seed for new games
    /// </summary>
    public void GenerateNewSeed()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
    }
}
