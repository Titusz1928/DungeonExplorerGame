using UnityEngine;
using UnityEngine.UI;

public class SaveButtonHandler : MonoBehaviour
{
    [SerializeField] private Button saveButton;

    private void Awake()
    {
        // Auto-assign if not set in inspector
        if (saveButton == null) saveButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (UIManager.Instance == null) return;

        // The button is interactable ONLY if we are NOT in battle
        bool canSave = !UIManager.Instance.IsInBattle;

        // Only update if state changed to save performance
        if (saveButton.interactable != canSave)
        {
            saveButton.interactable = canSave;
        }
    }

    public void OnSaveButtonPressed()
    {
        // Double check just in case
        if (UIManager.Instance.IsInBattle) return;

        Debug.Log("Save button pressed");
        MessageManager.Instance.ShowMessageDirectly("Game saved");
        SaveSystem.SaveGame();
    }
}