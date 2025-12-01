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


    public void LoadScene(string sceneName)
    {
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
