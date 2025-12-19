using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEX : MonoBehaviour
{
    public static SceneManagerEX Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /*    private void CleanupPersistentManagers()
        {
            var uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null) Destroy(uiManager.gameObject);

            var itemDb = FindFirstObjectByType<ItemDatabase>();
            if (itemDb != null) Destroy(itemDb.gameObject);
        }*/

    public static void CleanupGameSession()
    {
        if (GameBoot.PersistentPlayer != null)
            Destroy(GameBoot.PersistentPlayer);

        if (GameBoot.PersistentWorld != null)
            Destroy(GameBoot.PersistentWorld);

        // Reset references so GameBoot knows to recreate them next time
        GameBoot.PersistentPlayer = null;
        GameBoot.PersistentWorld = null;
    }


    public void LoadScene(string sceneName)
    {
        if(sceneName=="MainMenu")
            CleanupGameSession();

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
