using TitusGames.Framework;
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
        ServiceLocator.Current.TryGet<IUIManagerService>(out var uimanagerService);
        if (uimanagerService == null) return;

        // The button is interactable ONLY if we are NOT in battle
        bool canSave = !uimanagerService.IsInBattle;

        // Only update if state changed to save performance
        if (saveButton.interactable != canSave)
        {
            saveButton.interactable = canSave;
        }
    }

    public void OnSaveButtonPressed()
    {
        // Double check just in case
        ServiceLocator.Current.TryGet<IUIManagerService>(out var uimanagerService);
        if (uimanagerService.IsInBattle) return;

        Debug.Log("Save button pressed");
        ServiceLocator.Current.TryGet<IMessageService>(out var messageService);
        messageService.ShowMessageDirectly("Game saved");
        SaveSystem.SaveGame();
    }
}