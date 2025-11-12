using System;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Localization Settings")]
    [Tooltip("Folder inside Resources where language json files live (e.g. Resources/Languages)")]
    public string resourcesFolder = "Languages";

    [Tooltip("Language codes, must match filenames inside Resources/Languages (without extension). Order should match your dropdown indices.")]
    public string[] languageCodes = new string[] { "hun", "eng" };

    [Tooltip("Key used in PlayerPrefs for saving selected language index")]
    public string prefsKey = "languageIndex";

    private Dictionary<string, string> localizedText = new Dictionary<string, string>();
    public static event Action OnLanguageChanged;

    public string CurrentLanguageCode { get; private set; } = "eng";

    // --- Singleton setup ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Called by Boot.cs or other managers when starting up.
    /// Ensures a valid language is loaded.
    /// </summary>
    public void Initialize()
    {
        int savedIndex = PlayerPrefs.GetInt(prefsKey, 0);
        savedIndex = Mathf.Clamp(savedIndex, 0, languageCodes.Length - 1);
        LoadLanguage(languageCodes[savedIndex]);
        Debug.Log($"LocalizationManager initialized with language: {CurrentLanguageCode}");
    }

    /// <summary>
    /// Load language file (Resources/Languages/{languageCode}.json)
    /// </summary>
    public void LoadLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode))
        {
            Debug.LogError("LoadLanguage called with empty languageCode");
            return;
        }

        TextAsset txt = Resources.Load<TextAsset>($"{resourcesFolder}/{languageCode}");
        if (txt == null)
        {
            Debug.LogError($"Localization file not found: Resources/{resourcesFolder}/{languageCode}.json");
            return;
        }

        localizedText = ParseFlatJsonToDictionary(txt.text);
        CurrentLanguageCode = languageCode;
        Debug.Log($"Loaded language '{languageCode}' with {localizedText.Count} entries.");

        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Change language using dropdown index.
    /// </summary>
    public void SetLanguageIndex(int index)
    {
        if (index < 0 || index >= languageCodes.Length)
            return;

        PlayerPrefs.SetInt(prefsKey, index);
        PlayerPrefs.Save();
        LoadLanguage(languageCodes[index]);
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.TryGetValue(key, out string value))
            return value;
        return $"[MISSING:{key}]";
    }

    private Dictionary<string, string> ParseFlatJsonToDictionary(string json)
    {
        var dict = new Dictionary<string, string>();
        var raw = Json.Deserialize(json) as Dictionary<string, object>;
        if (raw != null)
        {
            foreach (var kvp in raw)
                dict[kvp.Key] = kvp.Value.ToString();
        }
        return dict;
    }
}

