using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("World")]
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

    /// <summary>
    /// Generate a new random seed for new games
    /// </summary>
    public void GenerateNewSeed()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
    }
}
