using UnityEngine;

public class SaveButtonHandler : MonoBehaviour
{
    public void OnSaveButtonPressed()
    {
        // Call the static save system
        Debug.Log("save button pressed");
        SaveSystem.SaveGame();
    }
}
