using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown dropdown;

    [Header("Language Settings")]
    // Swapped eng and hun so English is index 0
    public string[] languageCodes = { "eng", "spa", "hun", "rom" };

    private string PREF_KEY = "language";

    void Start()
    {
        // 1. Clear any placeholder options from the inspector design
        dropdown.ClearOptions();

        // 2. Load saved language (defaulting to 0 / English)
        int savedLang = PlayerPrefs.GetInt(PREF_KEY, 0);
        savedLang = Mathf.Clamp(savedLang, 0, languageCodes.Length - 1);

        // 3. Initialize
        PopulateDropdown();

        // Setting the value triggers the OnLanguageChanged logic if we aren't careful, 
        // so we set it after population.
        dropdown.value = savedLang;
        ApplyLanguage(savedLang);

        // 4. Listen to dropdown changes
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void PopulateDropdown()
    {
        dropdown.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < languageCodes.Length; i++)
        {
            // Fetch name from LocalizationManager
            string localizedName = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);

            // Fallback: If localization isn't ready, use the code itself so it's not "Option A"
            if (string.IsNullOrEmpty(localizedName)) localizedName = languageCodes[i];

            options.Add(new TMP_Dropdown.OptionData(localizedName));
        }

        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();
    }

    private void OnLanguageChanged(int index)
    {
        PlayerPrefs.SetInt(PREF_KEY, index);
        PlayerPrefs.Save();

        ApplyLanguage(index);

        // We update labels in case the names of languages themselves change per language
        UpdateDropdownLabels();
    }

    private void UpdateDropdownLabels()
    {
        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (i < dropdown.options.Count)
            {
                string localizedName = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
                dropdown.options[i].text = localizedName;
            }
        }
        dropdown.RefreshShownValue();
    }

    private void ApplyLanguage(int index)
    {
        // Corrected switch cases to match the new order (English = 0)
        switch (index)
        {
            case 0:
                Debug.Log("Set language to English");
                break;
            case 1:
                Debug.Log("Set language to Spanish");
                break;
            case 2:
                Debug.Log("Set language to Hungarian");
                break;
            case 3:
                Debug.Log("Set language to Romanian");
                break;
        }

        LocalizationManager.Instance.SetLanguageIndex(index);
    }
}