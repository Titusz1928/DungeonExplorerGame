using System.Collections.Generic;
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

    [Header("Token Settings")]
    [SerializeField] private int totalStartingTokens = 10;
    private int remainingTokens;

    [Header("UI References")]
    [SerializeField] private Transform skillRowContainer;
    [SerializeField] private GameObject skillRowPrefab;
    [SerializeField] private GameObject skillCellPrefab;
    [SerializeField] private TextMeshProUGUI tokensRemainingLabel;

    // Track selections temporarily
    private Dictionary<PlayerSkill, int> selectedSkills = new Dictionary<PlayerSkill, int>();
    private List<SkillTokenCell> spawnedCells = new List<SkillTokenCell>();

    private void Awake()
    {
        Instance = this;
        remainingTokens = totalStartingTokens;
        InitializeSkillUI();
    }

    private void InitializeSkillUI()
    {
        var skills = (PlayerSkill[])System.Enum.GetValues(typeof(PlayerSkill));
        GameObject currentRow = null;

        for (int i = 0; i < skills.Length; i++)
        {
            // Create a new row every 4 cells
            if (i % 4 == 0)
            {
                currentRow = Instantiate(skillRowPrefab, skillRowContainer);
            }

            GameObject cellGo = Instantiate(skillCellPrefab, currentRow.transform);
            SkillTokenCell cell = cellGo.GetComponent<SkillTokenCell>();

            selectedSkills[skills[i]] = 1;
            cell.Setup(skills[i]);
            spawnedCells.Add(cell);
        }
        UpdateTotalUI();
    }

    public void AdjustSkill(PlayerSkill skill, int amount)
    {
        if (amount > 0 && remainingTokens <= 0) return; // No tokens left
        if (amount < 0 && selectedSkills[skill] <= 1) return; // Already at 0

        selectedSkills[skill] += amount;
        remainingTokens -= amount;

        UpdateTotalUI();
    }

    private void UpdateTotalUI()
    {
        tokensRemainingLabel.text = $"Tokens: {remainingTokens}";
        foreach (var cell in spawnedCells) cell.UpdateUI();
    }

    public int GetSelectedLevel(PlayerSkill skill) => selectedSkills[skill];

    private void FinalizeSkillSettings()
    {
        // 1. Randomly distribute leftover tokens
        var skillList = new List<PlayerSkill>(selectedSkills.Keys);
        while (remainingTokens > 0)
        {
            PlayerSkill randomSkill = skillList[Random.Range(0, skillList.Count)];
            selectedSkills[randomSkill]++;
            remainingTokens--;
        }

        // 2. Save to GameSettings
        GameSettings.Instance.startingSkills.Clear();
        foreach (var kvp in selectedSkills)
        {
            GameSettings.Instance.startingSkills.Add(new SkillStartEntry { skill = kvp.Key, level = kvp.Value });
        }
    }

    // --- Entry Points ---

    public void StartNewGame()
    {
        PrepareSettings();
        FinalizeSkillSettings();

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