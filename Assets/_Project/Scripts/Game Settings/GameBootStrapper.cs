using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    void Start()
    {
        if (GameSettings.Instance == null)
        {
            Debug.LogError("GameSettings missing! Defaulting to new game.");
            StartNewGame();
            return;
        }

        if (GameSettings.Instance.loadFromSave)
        {
            Debug.Log("LOAD GAME requested");
            LoadGameStub();
        }
        else
        {
            Debug.Log("NEW GAME requested");
            StartNewGame();
        }
    }

    void StartNewGame()
    {
        Debug.Log("New game started");

        // Normal world generation path
        // Inventory empty
        // Player at spawn
    }

    void LoadGameStub()
    {
        Debug.Log("LoadGame not implemented yet — falling back to new game");

        // TEMP fallback
        StartNewGame();
    }
}
