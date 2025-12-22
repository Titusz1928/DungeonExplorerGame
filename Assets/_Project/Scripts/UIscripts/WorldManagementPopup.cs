using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldManagementPopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text seedText;
    [SerializeField] private TMP_Text createdAtText;
    [SerializeField] private TMP_Text lastPlayedAtText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;

    private WorldMeta currentMeta;

    // This is the function we call immediately after instantiation
    public void Initialize(WorldMeta meta)
    {
        currentMeta = meta;
        nameInput.text = meta.worldName;

        seedText.text = meta.seed.ToString();

        createdAtText.text = FormatISODate(meta.createdAt);
        lastPlayedAtText.text = FormatISODate(meta.updatedAt);

        // Clear listeners to avoid issues if the prefab is reused (pooling)
        saveButton.onClick.RemoveAllListeners();
        saveButton.onClick.AddListener(SaveAndClose);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(DeleteAndClose);
    }

    private string FormatISODate(string isoString)
    {
        if (DateTime.TryParse(isoString, out DateTime date))
        {
            // "yyyy-MM-dd" gives you 2025-12-22
            // "MMM dd, yyyy" would give you Dec 22, 2025
            return date.ToString("yyyy-MM-dd");
        }
        return "Unknown";
    }


    private void SaveAndClose()
    {
        currentMeta.worldName = nameInput.text;
        SaveSystem.UpdateWorldMeta(currentMeta);

        RefreshAndClose();
    }

    private void DeleteAndClose()
    {
        SaveSystem.DeleteWorld(currentMeta.worldId);
        RefreshAndClose();
    }

    private void RefreshAndClose()
    {
        // Tell the list to rebuild itself to show changes
        FindObjectOfType<ContinueGameSectionUI>().Toggle();
        FindObjectOfType<ContinueGameSectionUI>().Toggle();

        // Use your WindowManager logic to close/pop the window
        // Assuming your WindowManager has a Close() or similar
        Destroy(gameObject);
    }
}