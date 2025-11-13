using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown dropdown;

    [Header("Language Settings")]
    public string[] languageCodes = { "hun", "eng" }; // internal codes

    private string PREF_KEY = "language"; // key in PlayerPrefs

    void Start()
    {
        // Load saved language or default to 0
        int savedLang = PlayerPrefs.GetInt(PREF_KEY, 0);
        savedLang = Mathf.Clamp(savedLang, 0, languageCodes.Length - 1);

        PopulateDropdown();
        dropdown.value = savedLang;
        ApplyLanguage(savedLang);
        UpdateDropdownLabels();

        // Listen to dropdown changes
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void PopulateDropdown()
    {
        dropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < languageCodes.Length; i++)
        {
            string localizedName = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
            options.Add(new TMP_Dropdown.OptionData(localizedName));
        }

        dropdown.AddOptions(options);
    }

    private void UpdateDropdownLabels()
    {
        for (int i = 0; i < languageCodes.Length; i++)
        {
            if (i < dropdown.options.Count)
                dropdown.options[i].text = LocalizationManager.Instance.GetLocalizedValue(languageCodes[i]);
        }

        dropdown.RefreshShownValue();
    }

    private void OnLanguageChanged(int index)
    {
        PlayerPrefs.SetInt(PREF_KEY, index);
        PlayerPrefs.Save();

        ApplyLanguage(index);
        UpdateDropdownLabels();
    }

    private void ApplyLanguage(int index)
    {
        // Debug log for testing
        switch (index)
        {
            case 0:
                Debug.Log("Set language to Hungarian");
                break;
            case 1:
                Debug.Log("Set language to English");
                break;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTestSFX();

        // Update LocalizationManager
        LocalizationManager.Instance.SetLanguageIndex(index);
    }

    // Optional helper methods if you want buttons to switch languages
    public void SelectHungarian() => ApplyLanguage(0);
    public void SelectEnglish() => ApplyLanguage(1);
}
