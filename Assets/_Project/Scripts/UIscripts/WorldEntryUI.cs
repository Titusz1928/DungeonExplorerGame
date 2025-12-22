using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class WorldEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text worldNameText;
    [SerializeField] private TMP_Text lastPlayedText;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button editButton;

    [SerializeField] private GameObject managementWindowPrefab;
    private WorldMeta myMeta;

    private string worldId;

    public void Initialize(WorldMeta meta)
    {
        myMeta = meta;

        worldId = meta.worldId;
        worldNameText.text = meta.worldName;
        lastPlayedText.text = FormatLastPlayed(meta.updatedAt);

        // Clear existing listeners to prevent double-calls if prefabs are pooled
        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(OnLoadClicked);

        editButton.onClick.RemoveAllListeners();
        editButton.onClick.AddListener(OpenEditWindow);
    }

    private void OpenEditWindow()
    {
        // 1. Tell WindowManager to instantiate the prefab
        GameObject windowObj = WindowManager.Instance.OpenWindow(managementWindowPrefab);

        // 2. Get the script component from the instantiated object
        WorldManagementPopup popupScript = windowObj.GetComponent<WorldManagementPopup>();

        // 3. Pass the data!
        if (popupScript != null)
        {
            popupScript.Initialize(myMeta);
        }
    }

    private void OnLoadClicked()
    {
        // Simply pass the ID to the controller
        GameSessionController.Instance.LoadExistingWorld(worldId);
    }

    private string FormatLastPlayed(string iso)
    {
        if (!DateTime.TryParse(iso, out var date)) return "Last played: Unknown";

        TimeSpan diff = DateTime.UtcNow - date;
        if (diff.TotalDays >= 1) return $"Last played: {(int)diff.TotalDays}d ago";
        if (diff.TotalHours >= 1) return $"Last played: {(int)diff.TotalHours}h ago";

        return "Last played: Recently";
    }
}