using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionController : MonoBehaviour
{
    [SerializeField] private GameSettingsDefaults defaults;
    [SerializeField] private string gameplaySceneName = "GameBoot";
    [SerializeField] private TMP_InputField seedInput;

    public void StartNewGame()
    {
        EnsureGameSettings();

        // 1. Get the raw string from UI
        string rawInput = seedInput.text;
        int finalSeed;

        if (string.IsNullOrWhiteSpace(rawInput))
        {
            // Case A: Empty input -> Random seed
            finalSeed = Random.Range(int.MinValue, int.MaxValue);
        }
        else if (int.TryParse(rawInput, out int numericSeed))
        {
            // Case B: User typed a valid number -> Use it directly
            finalSeed = numericSeed;
        }
        else
        {
            // Case C: User typed text -> Hash it to a number
            // GetHashCode() returns a deterministic integer for any string
            finalSeed = rawInput.GetHashCode();
        }

        // Apply the seed and settings
        GameSettings.Instance.seed = finalSeed;
        GameSettings.Instance.difficulty = defaults.defaultDifficulty;
        GameSettings.Instance.loadFromSave = false;

        Debug.Log($"Starting game with seed: {finalSeed}");
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void LoadGame()
    {
        EnsureGameSettings();

        GameSettings.Instance.loadFromSave = true;

        SceneManager.LoadScene(gameplaySceneName);
    }

    private void EnsureGameSettings()
    {
        if (GameSettings.Instance == null)
        {
            GameObject go = new GameObject("GameSettings");
            go.AddComponent<GameSettings>();
        }
    }
}
