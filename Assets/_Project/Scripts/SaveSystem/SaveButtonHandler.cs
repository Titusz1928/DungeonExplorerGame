using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SaveButtonHandler : MonoBehaviour
{
    public void OnSaveButtonPressed()
    {
        // Call the static save system
        Debug.Log("save button pressed");

        MessageManager.Instance.ShowMessageDirectly($"Game saved");

        SaveSystem.SaveGame();
    }
}
