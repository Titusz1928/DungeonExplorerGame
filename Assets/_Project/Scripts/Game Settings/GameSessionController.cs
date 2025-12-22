using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionController : MonoBehaviour
{
    // Singleton pattern for easy access from UI entries
    public static GameSessionController Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameSettingsDefaults defaults;
    [SerializeField] private string gameplaySceneName = "GameBoot";

    [Header("New Game UI")]
    [SerializeField] private TMP_InputField seedInput;
    [SerializeField] private TMP_InputField nameInput;

    private void Awake()
    {
        Instance = this;
    }

    // --- Entry Points ---

    public void StartNewGame()
    {
        PrepareSettings();

        int finalSeed = ResolveSeedFromInput();
        GameSettings.Instance.CreateNewWorld(nameInput.text, finalSeed);
        GameSettings.Instance.difficulty = defaults.defaultDifficulty;

        Launch();
    }

    public void LoadExistingWorld(string worldId)
    {
        PrepareSettings();

        GameSettings.Instance.worldId = worldId;
        GameSettings.Instance.loadFromSave = true;

        Launch();
    }

    // --- Helper Logic ---

    private void PrepareSettings()
    {
        // Centralized check: ensure GameSettings exists before modifying it
        if (GameSettings.Instance == null)
        {
            new GameObject("GameSettings", typeof(GameSettings));
        }
    }

    private void Launch()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    private int ResolveSeedFromInput()
    {
        string rawInput = seedInput?.text;
        if (string.IsNullOrWhiteSpace(rawInput))
            return Random.Range(int.MinValue, int.MaxValue);

        return int.TryParse(rawInput, out int numericSeed)
            ? numericSeed
            : rawInput.GetHashCode();
    }
}